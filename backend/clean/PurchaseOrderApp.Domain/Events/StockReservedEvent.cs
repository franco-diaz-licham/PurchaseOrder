using PurchaseOrderApp.Domain.Core;
using PurchaseOrderApp.Domain.ValueObjects;

namespace PurchaseOrderApp.Domain.Events;

/// <summary>
/// Raised when stock is reserved against a purchase order line.
/// </summary>
/// <param name="StockReservationId">Reservation created by the stock reservation action.</param>
/// <param name="PurchaseOrderLineId">Purchase order line that received the reservation.</param>
/// <param name="WarehouseId">Warehouse where stock was reserved.</param>
/// <param name="InventoryItemId">Inventory item that was reserved.</param>
/// <param name="QuantityReserved">Quantity reserved by the action.</param>
/// <param name="ResultingAvailableQuantity">Available stock after the reservation completed.</param>
/// <param name="User">User or system actor that reserved the stock.</param>
/// <param name="OccurredAt">Date and time the reservation occurred.</param>
public sealed record StockReservedEvent(
    StockReservationId StockReservationId,
    PurchaseOrderLineId PurchaseOrderLineId,
    WarehouseId WarehouseId,
    InventoryItemId InventoryItemId,
    Quantity QuantityReserved,
    Quantity ResultingAvailableQuantity,
    string User,
    DateTimeOffset OccurredAt) : IDomainEvent;
