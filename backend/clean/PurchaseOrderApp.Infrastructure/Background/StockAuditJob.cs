using Hangfire;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PurchaseOrderApp.Domain.Entities;
using PurchaseOrderApp.Domain.ValueObjects;

namespace PurchaseOrderApp.Infrastructure.Background;

public sealed class StockAuditJob(DatabaseContext db)
{
    [Queue("audit")]
    [AutomaticRetry(Attempts = 5, DelaysInSeconds = new[] { 1, 5, 15, 60, 300 })]
    [JobDisplayName("Record stock audit ({0})")]
    public async Task ExecuteAsync(Guid messageId, CancellationToken cancellationToken)
    {
        var auditId = new AuditLogEntryId(messageId);
        if (await db.AuditLogEntries.AnyAsync(entry => entry.Id == auditId, cancellationToken)) return;
        var message = await db.OutboxMessages.AsNoTracking().SingleAsync(row => row.Id == messageId, cancellationToken);
        var entry = message.MessageType switch {
            AuditLogOutboxMapper.StockReservedMessageType => AuditLogEntry.RecordReservation(
                AuditLogOutboxMapper.ToStockReservedEvent(message.Payload), auditId),
            AuditLogOutboxMapper.StockReleasedMessageType => AuditLogEntry.RecordRelease(
                AuditLogOutboxMapper.ToStockReleasedEvent(message.Payload), auditId),
            _ => throw new InvalidOperationException($"No audit handler is registered for '{message.MessageType}'.")
        };
        db.AuditLogEntries.Add(entry);
        try {
            await db.SaveChangesAsync(cancellationToken);
        } catch (DbUpdateException ex) when (ex.InnerException is PostgresException {
            SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: "pk_audit_log_entries"
        }) {
            // A concurrent execution of this same event already inserted its audit entry.
            db.Entry(entry).State = EntityState.Detached;
            if (!await db.AuditLogEntries.AnyAsync(existing => existing.Id == auditId, cancellationToken)) throw;
        }
    }
}
