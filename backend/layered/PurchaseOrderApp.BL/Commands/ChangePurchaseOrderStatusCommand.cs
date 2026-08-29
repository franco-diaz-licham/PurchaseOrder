namespace PurchaseOrderApp.BL.Commands;

public sealed record ChangePurchaseOrderStatusCommand(
    Guid PurchaseOrderId,
    string User,
    DateTimeOffset OccurredAt);

public sealed record SubmitPurchaseOrderCommand(
    Guid WarehouseId,
    List<SubmitPurchaseOrderLineCommand> Lines,
    string User,
    DateTimeOffset OccurredAt);

public sealed record SubmitPurchaseOrderLineCommand(
    Guid InventoryItemId,
    decimal QuantityOrdered);

public sealed record AddPurchaseOrderLineCommand(
    Guid PurchaseOrderId,
    Guid InventoryItemId,
    decimal QuantityOrdered,
    string User,
    DateTimeOffset OccurredAt);

public sealed record RemovePurchaseOrderLineCommand(
    Guid PurchaseOrderId,
    Guid PurchaseOrderLineId,
    string User,
    DateTimeOffset OccurredAt);

public sealed record UpdatePurchaseOrderLineCommand(
    Guid PurchaseOrderId,
    Guid PurchaseOrderLineId,
    decimal QuantityOrdered,
    string User,
    DateTimeOffset OccurredAt);

public sealed record CreateReservationCommand(
    Guid PurchaseOrderLineId,
    Guid WarehouseId,
    decimal Quantity,
    string User,
    DateTimeOffset OccurredAt);

public sealed record ReleaseReservationCommand(
    Guid StockReservationId,
    decimal Quantity,
    string User,
    DateTimeOffset OccurredAt);

public sealed record ChangeInventoryItemStandardCostCommand(
    Guid InventoryItemId,
    decimal StandardCost,
    string User,
    DateTimeOffset OccurredAt);
