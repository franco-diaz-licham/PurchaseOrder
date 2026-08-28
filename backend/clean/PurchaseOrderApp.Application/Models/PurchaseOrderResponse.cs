namespace PurchaseOrderApp.Application.Models;

public sealed record PurchaseOrderResponse(
    Guid PurchaseOrderId,
    string PurchaseOrderNumber,
    Guid WarehouseId,
    string Status,
    decimal SubtotalAmount,
    decimal GstAmount,
    decimal TotalAmount,
    List<PurchaseOrderLineResponse> Lines);

public sealed record PurchaseOrderLineResponse(
    Guid PurchaseOrderLineId,
    Guid InventoryItemId,
    decimal QuantityOrdered,
    decimal QuantityReserved,
    decimal QuantityRemaining,
    decimal UnitCost,
    decimal LineAmount);
