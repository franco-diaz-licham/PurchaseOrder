using PurchaseOrderApp.Application.Models;
using PurchaseOrderApp.Domain.Entities;
using PurchaseOrderApp.Domain.Enums;
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
    /// Gets a stock reservation read model by id.
    /// </summary>
    Task<ReservationResponse?> GetResponseAsync(StockReservationId stockReservationId, CancellationToken cancellationToken);

    /// <summary>
    /// Lists stock reservation read models.
    /// </summary>
    Task<List<ReservationResponse>> ListResponsesAsync(WarehouseId? warehouseId, ReservationStatus? status, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the active reserved quantity for one inventory item at one warehouse.
    /// </summary>
    Task<Quantity> GetActiveReservedQuantityAsync(WarehouseId warehouseId, InventoryItemId inventoryItemId, CancellationToken cancellationToken);

    /// <summary>
    /// Lists active reservations for one purchase order line.
    /// </summary>
    Task<List<StockReservation>> ListActiveByLineAsync(PurchaseOrderLineId purchaseOrderLineId, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a stock reservation to the current unit of work.
    /// </summary>
    Task AddAsync(StockReservation stockReservation, CancellationToken cancellationToken);
}
