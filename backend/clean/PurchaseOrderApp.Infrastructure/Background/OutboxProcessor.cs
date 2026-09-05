using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PurchaseOrderApp.Application.Ports;

namespace PurchaseOrderApp.Infrastructure.Background;

/// <summary>Publishes committed outbox rows; Hangfire executes and retries each audit job.</summary>
public sealed class OutboxProcessor(
    DatabaseContext db,
    IAuditJobQueue queue,
    ILogger<OutboxProcessor> logger) : IOutboxProcessor
{
    public async Task ProcessPendingAsync(CancellationToken cancellationToken)
    {
        var messages = await db.OutboxMessages.AsNoTracking()
            .Where(message => message.PublishedUtc == null)
            .OrderBy(message => message.CreatedUtc)
            .Take(20)
            .ToListAsync(cancellationToken);

        foreach (var message in messages) {
            cancellationToken.ThrowIfCancellationRequested();
            try {
                var jobId = queue.Enqueue(message.Id);
                await db.OutboxMessages
                    .Where(row => row.Id == message.Id && row.PublishedUtc == null)
                    .ExecuteUpdateAsync(update => update
                        .SetProperty(row => row.PublishedUtc, DateTimeOffset.UtcNow)
                        .SetProperty(row => row.HangfireJobId, jobId), cancellationToken);
            } catch (Exception ex) when (!cancellationToken.IsCancellationRequested) {
                // Leave unpublished: the next cron run tries again.
                logger.LogWarning(ex, "Could not publish outbox message {MessageId}", message.Id);
            }
        }
    }
}