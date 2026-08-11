using PurchaseOrder.Domain.Entities;
using PurchaseOrder.Domain.ValueObjects;

namespace PurchaseOrder.Application.Ports;

/// <summary>
/// Loads and persists warehouse aggregate roots.
/// </summary>
public interface IWarehouseRepository
{
    /// <summary>
    /// Gets a warehouse by id.
    /// </summary>
    Task<Warehouse?> GetAsync(WarehouseId warehouseId, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a warehouse to the current unit of work.
    /// </summary>
    Task AddAsync(Warehouse warehouse, CancellationToken cancellationToken);
}
