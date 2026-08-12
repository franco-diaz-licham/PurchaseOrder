namespace PurchaseOrderApp.Api.Controllers.Models;

public sealed record SubmitPurchaseOrderRequest(
    string PurchaseOrderNumber,
    Guid WarehouseId,
    List<SubmitPurchaseOrderLineRequest>? Lines,
    string User);

public sealed record SubmitPurchaseOrderLineRequest(
    Guid InventoryItemId,
    decimal QuantityOrdered);

public sealed record AddPurchaseOrderLineRequest(
    Guid InventoryItemId,
    decimal QuantityOrdered,
    string User);

public sealed record ChangePurchaseOrderStatusRequest(string User);
