using Hangfire;
using PurchaseOrderApp.Application.Ports;

namespace PurchaseOrderApp.Infrastructure.Background;

public sealed class HangfireAuditJobQueue(IBackgroundJobClient client) : IAuditJobQueue
{
    public string Enqueue(Guid messageId)
    {
        var jobId = client.Enqueue<StockAuditJob>(job => job.ExecuteAsync(messageId, CancellationToken.None));
        return !string.IsNullOrWhiteSpace(jobId)
            ? jobId
            : throw new InvalidOperationException("The audit job was not persisted.");
    }
}