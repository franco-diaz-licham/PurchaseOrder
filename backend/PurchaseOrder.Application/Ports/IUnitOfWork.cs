namespace PurchaseOrder.Application.Ports;

/// <summary>
/// Coordinates persistence and transaction boundaries for application use cases.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Saves pending changes in the current unit of work.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Begins a database transaction when one is not already active.
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the active database transaction.
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the active database transaction.
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
