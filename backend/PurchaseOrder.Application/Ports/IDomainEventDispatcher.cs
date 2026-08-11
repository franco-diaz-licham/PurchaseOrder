using PurchaseOrder.Domain.Core;

namespace PurchaseOrder.Application.Ports;

/// <summary>
/// Dispatches domain events raised by aggregates during the current unit of work.
/// </summary>
public interface IDomainEventDispatcher
{
    /// <summary>
    /// Dispatches domain events collected from changed aggregates.
    /// </summary>
    Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken);
}
