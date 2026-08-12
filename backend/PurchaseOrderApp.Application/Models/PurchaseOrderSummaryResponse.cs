namespace PurchaseOrderApp.Application.Models;

public sealed record PurchaseOrderSummaryResponse(
    Guid PurchaseOrderId,
    string PurchaseOrderNumber,
    Guid WarehouseId,
    string Status,
    int LineCount,
    decimal QuantityOrdered,
    decimal QuantityReserved,
    decimal QuantityRemaining,
    decimal SubtotalAmount,
    decimal GstAmount,
    decimal TotalAmount);
