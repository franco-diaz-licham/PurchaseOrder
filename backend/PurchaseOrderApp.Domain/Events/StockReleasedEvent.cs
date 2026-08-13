using PurchaseOrderApp.Domain.Core;
using PurchaseOrderApp.Domain.ValueObjects;

namespace PurchaseOrderApp.Domain.Events;

/// <summary>
/// Raised when reserved stock is released from a purchase order line.
/// </summary>
/// <param name="StockReservationId">Reservation affected by the release action.</param>
/// <param name="PurchaseOrderLineId">Purchase order line affected by the release.</param>
/// <param name="WarehouseId">Warehouse where stock was released.</param>
/// <param name="InventoryItemId">Inventory item that was released.</param>
/// <param name="QuantityReleased">Quantity released by the action.</param>
/// <param name="ResultingAvailableQuantity">Available stock after the release completed.</param>
/// <param name="User">User or system actor that released the stock.</param>
/// <param name="OccurredAt">Date and time the release occurred.</param>
public sealed record StockReleasedEvent(
    StockReservationId StockReservationId,
    PurchaseOrderLineId PurchaseOrderLineId,
    WarehouseId WarehouseId,
    InventoryItemId InventoryItemId,
    Quantity QuantityReleased,
    Quantity ResultingAvailableQuantity,
    string User,
    DateTimeOffset OccurredAt) : IDomainEvent;
