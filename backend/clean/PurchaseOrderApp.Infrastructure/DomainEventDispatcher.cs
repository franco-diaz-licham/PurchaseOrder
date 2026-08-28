using PurchaseOrderApp.Application.Ports;
using PurchaseOrderApp.Domain.Core;
using PurchaseOrderApp.Domain.Entities;
using PurchaseOrderApp.Domain.Events;

namespace PurchaseOrderApp.Infrastructure;

public sealed class DomainEventDispatcher(DatabaseContext db) : IDomainEventDispatcher
{
    public async Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken)
    {
        foreach (var domainEvent in domainEvents) {
            switch (domainEvent) {
                case StockReservedEvent stockReserved:
                    await db.AuditLogEntries.AddAsync(AuditLogEntry.RecordReservation(stockReserved), cancellationToken);
                    break;
                case StockReleasedEvent stockReleased:
                    await db.AuditLogEntries.AddAsync(AuditLogEntry.RecordRelease(stockReleased), cancellationToken);
                    break;
            }
        }
    }
}
