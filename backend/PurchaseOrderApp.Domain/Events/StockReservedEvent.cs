using PurchaseOrderApp.Domain.Core;
using PurchaseOrderApp.Domain.ValueObjects;

namespace PurchaseOrderApp.Domain.Events;

/// <summary>
/// Raised when stock is reserved against a purchase order line.
/// </summary>
public sealed record StockReservedEvent(
    StockReservationId StockReservationId,
    PurchaseOrderLineId PurchaseOrderLineId,
    WarehouseId WarehouseId,
    InventoryItemId InventoryItemId,
    Quantity QuantityReserved,
    Quantity ResultingAvailableQuantity,
    string User,
    DateTimeOffset OccurredAt) : IDomainEvent;
