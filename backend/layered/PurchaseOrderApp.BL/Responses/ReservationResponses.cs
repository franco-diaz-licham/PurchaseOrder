namespace PurchaseOrderApp.BL.Responses;

public sealed record ReservationResponse(
    Guid StockReservationId,
    Guid PurchaseOrderLineId,
    Guid WarehouseId,
    Guid InventoryItemId,
    decimal QuantityReserved,
    decimal UnitCostSnapshot,
    string Status,
    string ReservedBy,
    DateTimeOffset ReservedAt);

public sealed record AuditLogResponse(
    Guid AuditLogEntryId,
    string Action,
    Guid InventoryItemId,
    Guid WarehouseId,
    Guid PurchaseOrderLineId,
    Guid StockReservationId,
    decimal Quantity,
    decimal ResultingAvailableQuantity,
    string User,
    DateTimeOffset Timestamp);
