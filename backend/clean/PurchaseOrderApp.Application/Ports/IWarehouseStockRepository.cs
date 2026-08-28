using PurchaseOrderApp.Application.Models;
using PurchaseOrderApp.Domain.Entities;
using PurchaseOrderApp.Domain.ValueObjects;

namespace PurchaseOrderApp.Application.Ports;

/// <summary>
/// Loads warehouse stock read models and aggregate roots.
/// </summary>
public interface IWarehouseStockRepository
{
    /// <summary>
    /// Lists warehouse stock balances as read models for one warehouse.
    /// </summary>
    Task<List<WarehouseStockResponse>> ListResponsesAsync(WarehouseId warehouseId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets warehouse stock with a database row lock for reservation concurrency.
    /// </summary>
    Task<WarehouseStock?> GetForUpdateAsync(WarehouseId warehouseId, InventoryItemId inventoryItemId, CancellationToken cancellationToken);

}
