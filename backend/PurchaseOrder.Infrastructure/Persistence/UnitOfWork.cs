using Microsoft.EntityFrameworkCore.Storage;
using PurchaseOrder.Application.Ports;
using PurchaseOrder.Domain.Core;

namespace PurchaseOrder.Infrastructure.Persistence;

public sealed class UnitOfWork(DatabaseContext db, IDomainEventDispatcher domainEventDispatcher) : IUnitOfWork
{
    private IDbContextTransaction? transaction;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        var entitiesWithEvents = db.ChangeTracker
            .Entries<Entity>()
            .Where(entry => entry.Entity.DomainEvents.Count > 0)
            .Select(entry => entry.Entity)
            .ToList();

        var domainEvents = entitiesWithEvents
            .SelectMany(entity => entity.DomainEvents)
            .ToList();

        if (domainEvents.Count > 0) await domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);
        var result = await db.SaveChangesAsync(cancellationToken);
        foreach (var entity in entitiesWithEvents) entity.ClearDomainEvents();
        return result;
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        transaction ??= await db.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (transaction is null) return;

        await transaction.CommitAsync(cancellationToken);
        await transaction.DisposeAsync();
        transaction = null;
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (transaction is null) return;

        await transaction.RollbackAsync(cancellationToken);
        await transaction.DisposeAsync();
        transaction = null;
    }
}
