using System.Data;
using Microsoft.EntityFrameworkCore;
using PurchaseOrderApp.Application.Ports;
using PurchaseOrderApp.Domain.Entities;

namespace PurchaseOrderApp.Infrastructure.Background;

public sealed class OutboxProcessor(DatabaseContext db) : IOutboxProcessor
{
    private const int BatchSize = 20;
    private const int MaxAttempts = 5;
    private static readonly TimeSpan LeaseDuration = TimeSpan.FromMinutes(5);
    private readonly string _workerId = $"{Environment.MachineName}:{Guid.NewGuid():N}";

    public async Task ProcessPendingAsync(CancellationToken cancellationToken)
    {
        var messages = await ClaimMessagesAsync(cancellationToken);
        foreach (var message in messages) await ProcessMessageAsync(message, cancellationToken);
    }

    private async Task<List<OutboxMessage>> ClaimMessagesAsync(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var lockedUntil = now.Add(LeaseDuration);
        var claimedIds = new List<Guid>();
        var connection = db.Database.GetDbConnection();
        var shouldCloseConnection = connection.State != ConnectionState.Open;

        if (shouldCloseConnection) await connection.OpenAsync(cancellationToken);

        try {
            await using var command = connection.CreateCommand();
            command.CommandText = """
                UPDATE background.outbox_message
                SET status = 'processing',
                    attempt_count = attempt_count + 1,
                    locked_by = @workerId,
                    locked_until_utc = @lockedUntil,
                    updated_utc = @now
                WHERE id IN (
                    SELECT id
                    FROM background.outbox_message
                    WHERE status IN ('pending', 'failed')
                      AND next_attempt_utc <= @now
                      AND (locked_until_utc IS NULL OR locked_until_utc <= @now)
                    ORDER BY next_attempt_utc, occurred_utc
                    LIMIT @batchSize
                    FOR UPDATE SKIP LOCKED
                )
                RETURNING id
                """;

            AddParameter(command, "workerId", _workerId);
            AddParameter(command, "lockedUntil", lockedUntil);
            AddParameter(command, "now", now);
            AddParameter(command, "batchSize", BatchSize);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken)) claimedIds.Add(reader.GetGuid(0));
        } finally {
            if (shouldCloseConnection) await connection.CloseAsync();
        }

        if (claimedIds.Count == 0) return [];

        var messages = await db.OutboxMessages
            .Where(message => claimedIds.Contains(message.Id))
            .ToListAsync(cancellationToken);

        return claimedIds
            .Select(id => messages.Single(message => message.Id == id))
            .ToList();
    }

    private static void AddParameter(System.Data.Common.DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }

    private async Task ProcessMessageAsync(OutboxMessage claimedMessage, CancellationToken cancellationToken)
    {
        var message = await db.OutboxMessages.SingleAsync(outboxMessage => outboxMessage.Id == claimedMessage.Id, cancellationToken);

        try {
            await HandleMessageAsync(message, cancellationToken);
            message.MarkProcessed(DateTimeOffset.UtcNow);
        } catch (Exception ex) {
            message.MarkFailed(ex.Message, DateTimeOffset.UtcNow, MaxAttempts);
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task HandleMessageAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        var entry = message.MessageType switch {
            AuditLogOutboxMapper.StockReservedMessageType => AuditLogEntry.RecordReservation(AuditLogOutboxMapper.ToStockReservedEvent(message.Payload)),
            AuditLogOutboxMapper.StockReleasedMessageType => AuditLogEntry.RecordRelease(AuditLogOutboxMapper.ToStockReleasedEvent(message.Payload)),
            _ => throw new InvalidOperationException($"No outbox handler is registered for message type '{message.MessageType}'.")
        };

        await db.AuditLogEntries.AddAsync(entry, cancellationToken);
    }
}
