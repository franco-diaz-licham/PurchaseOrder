namespace PurchaseOrderApp.Application.Ports;

/// <summary>Enqueues a committed outbox message and returns the queue's job identifier.</summary>
public interface IAuditJobQueue
{
    string Enqueue(Guid messageId);
}