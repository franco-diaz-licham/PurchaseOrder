using PurchaseOrder.Application.Ports;
using PurchaseOrder.Domain.Core;

namespace PurchaseOrder.Infrastructure.Persistence;

public sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    public Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
