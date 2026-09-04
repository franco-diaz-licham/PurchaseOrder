using PurchaseOrderApp.Application.Ports;
using PurchaseOrderApp.Domain.Core;
using PurchaseOrderApp.Domain.Events;
using PurchaseOrderApp.Infrastructure.Background;

namespace PurchaseOrderApp.Infrastructure;

public sealed class DomainEventDispatcher(DatabaseContext db) : IDomainEventDispatcher
{
    public async Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken)
    {
        foreach (var domainEvent in domainEvents) {
            switch (domainEvent) {
                case StockReservedEvent stockReserved:
                    await db.OutboxMessages.AddAsync(AuditLogOutboxMapper.ToOutboxMessage(stockReserved), cancellationToken);
                    break;
                case StockReleasedEvent stockReleased:
                    await db.OutboxMessages.AddAsync(AuditLogOutboxMapper.ToOutboxMessage(stockReleased), cancellationToken);
                    break;
            }
        }
    }
}
