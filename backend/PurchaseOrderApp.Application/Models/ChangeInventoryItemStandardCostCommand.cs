using PurchaseOrderApp.Domain.ValueObjects;

namespace PurchaseOrderApp.Application.Models;

public sealed record ChangeInventoryItemStandardCostCommand(
    InventoryItemId InventoryItemId,
    Money StandardCost,
    string User,
    DateTimeOffset OccurredAt);
