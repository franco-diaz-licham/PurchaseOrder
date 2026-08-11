namespace PurchaseOrderApp.Application.Models;

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
