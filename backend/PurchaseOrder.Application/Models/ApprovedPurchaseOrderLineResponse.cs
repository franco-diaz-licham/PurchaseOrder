namespace PurchaseOrder.Application.Models;

public sealed record ApprovedPurchaseOrderLineResponse(
    Guid PurchaseOrderId,
    string PurchaseOrderNumber,
    Guid PurchaseOrderLineId,
    Guid WarehouseId,
    string WarehouseCode,
    string WarehouseName,
    Guid InventoryItemId,
    string Sku,
    string ItemName,
    decimal QuantityOrdered,
    decimal QuantityReserved,
    decimal QuantityRemaining);
