namespace PurchaseOrderApp.Application.Models;

public sealed record InventoryItemResponse(
    Guid InventoryItemId,
    string Sku,
    string Name,
    string Category,
    string TrackingMode,
    decimal StandardCost);
