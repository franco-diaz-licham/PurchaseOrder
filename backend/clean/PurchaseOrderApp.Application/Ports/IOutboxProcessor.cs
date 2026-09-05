namespace PurchaseOrderApp.Application.Ports;

/// <summary>
/// Relays committed outbox messages to the background queue.
/// </summary>
public interface IOutboxProcessor
{
    /// <summary>
    /// Publishes unpublished messages as individual background jobs.
    /// </summary>
    Task ProcessPendingAsync(CancellationToken cancellationToken);
}
