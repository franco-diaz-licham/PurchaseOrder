using PurchaseOrderApp.Application.Ports;
using PurchaseOrderApp.Domain.Core;

namespace PurchaseOrderApp.Infrastructure.Persistence;

public sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    public Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
