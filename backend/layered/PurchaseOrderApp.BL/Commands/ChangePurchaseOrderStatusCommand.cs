namespace PurchaseOrderApp.BL.Commands;

public sealed record ChangePurchaseOrderStatusCommand(
    Guid PurchaseOrderId,
    string User,
    DateTimeOffset OccurredAt);
