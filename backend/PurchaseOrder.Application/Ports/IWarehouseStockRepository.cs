using PurchaseOrder.Domain.Entities;
using PurchaseOrder.Domain.ValueObjects;

namespace PurchaseOrder.Application.Ports;

/// <summary>
/// Loads and persists warehouse stock aggregate roots.
/// </summary>
public interface IWarehouseStockRepository
{
    /// <summary>
    /// Gets warehouse stock for one warehouse and inventory item.
    /// </summary>
    Task<WarehouseStock?> GetAsync(WarehouseId warehouseId, InventoryItemId inventoryItemId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets warehouse stock with a database row lock for reservation concurrency.
    /// </summary>
    Task<WarehouseStock?> GetForUpdateAsync(WarehouseId warehouseId, InventoryItemId inventoryItemId, CancellationToken cancellationToken);

    /// <summary>
    /// Adds warehouse stock to the current unit of work.
    /// </summary>
    Task AddAsync(WarehouseStock warehouseStock, CancellationToken cancellationToken);
}
