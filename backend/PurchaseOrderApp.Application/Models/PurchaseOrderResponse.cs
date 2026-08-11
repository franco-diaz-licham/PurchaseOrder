namespace PurchaseOrderApp.Application.Models;

public sealed record PurchaseOrderResponse(
    Guid PurchaseOrderId,
    string PurchaseOrderNumber,
    Guid WarehouseId,
    string Status,
    List<PurchaseOrderLineResponse> Lines);

public sealed record PurchaseOrderLineResponse(
    Guid PurchaseOrderLineId,
    Guid InventoryItemId,
    decimal QuantityOrdered,
    decimal QuantityReserved,
    decimal QuantityRemaining);
