namespace PurchaseOrderApp.Application.Ports;

/// <summary>
/// Processes committed outbox messages into their eventual side effects.
/// </summary>
public interface IOutboxProcessor
{
    /// <summary>
    /// Claims and processes pending outbox messages.
    /// </summary>
    Task ProcessPendingAsync(CancellationToken cancellationToken);
}
