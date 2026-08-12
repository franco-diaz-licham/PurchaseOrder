using PurchaseOrderApp.Domain.Core;
using PurchaseOrderApp.Domain.ValueObjects;

namespace PurchaseOrderApp.Domain.Events;

/// <summary>
/// Raised when reserved stock is released from a purchase order line.
/// </summary>
public sealed record StockReleasedEvent(
    StockReservationId StockReservationId,
    PurchaseOrderLineId PurchaseOrderLineId,
    WarehouseId WarehouseId,
    InventoryItemId InventoryItemId,
    Quantity QuantityReleased,
    Quantity ResultingAvailableQuantity,
    string User,
    DateTimeOffset OccurredAt) : IDomainEvent;
