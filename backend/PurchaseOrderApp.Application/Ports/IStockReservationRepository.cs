using PurchaseOrderApp.Domain.Entities;
using PurchaseOrderApp.Domain.ValueObjects;

namespace PurchaseOrderApp.Application.Ports;

/// <summary>
/// Loads and persists stock reservation aggregate roots.
/// </summary>
public interface IStockReservationRepository
{
    /// <summary>
    /// Gets a stock reservation by id.
    /// </summary>
    Task<StockReservation?> GetAsync(StockReservationId stockReservationId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the active reserved quantity for one inventory item at one warehouse.
    /// </summary>
    Task<Quantity> GetActiveReservedQuantityAsync(WarehouseId warehouseId, InventoryItemId inventoryItemId, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a stock reservation to the current unit of work.
    /// </summary>
    Task AddAsync(StockReservation stockReservation, CancellationToken cancellationToken);
}
