namespace PurchaseOrderApp.Services.Models;

public sealed record ChangePurchaseOrderStatusRequest(string User);

public sealed record SubmitPurchaseOrderRequest(
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

public sealed record RemovePurchaseOrderLineRequest(string User);

public sealed record UpdatePurchaseOrderLineRequest(
    decimal QuantityOrdered,
    string User);

public sealed record CreateReservationRequest(
    Guid PurchaseOrderLineId,
    Guid WarehouseId,
    decimal Quantity,
    string User);

public sealed record ReleaseReservationRequest(
    decimal Quantity,
    string User);

public sealed record ChangeInventoryItemStandardCostRequest(
    decimal StandardCost,
    string User);
