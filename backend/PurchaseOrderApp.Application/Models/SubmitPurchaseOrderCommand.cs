using PurchaseOrderApp.Domain.ValueObjects;

namespace PurchaseOrderApp.Application.Models;

public sealed record SubmitPurchaseOrderCommand(
    string PurchaseOrderNumber,
    WarehouseId WarehouseId,
    List<SubmitPurchaseOrderLineCommand> Lines,
    string User,
    DateTimeOffset OccurredAt);

public sealed record SubmitPurchaseOrderLineCommand(
    InventoryItemId InventoryItemId,
    Quantity QuantityOrdered);

public sealed record AddPurchaseOrderLineCommand(
    PurchaseOrderId PurchaseOrderId,
    InventoryItemId InventoryItemId,
    Quantity QuantityOrdered,
    string User,
    DateTimeOffset OccurredAt);

public sealed record ChangePurchaseOrderStatusCommand(
    PurchaseOrderId PurchaseOrderId,
    string User,
    DateTimeOffset OccurredAt);
