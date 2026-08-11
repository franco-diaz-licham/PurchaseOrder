namespace PurchaseOrderApp.Domain.ValueObjects;

/// <summary>
/// Strongly typed identifier for an inventory item.
/// </summary>
public readonly record struct InventoryItemId(Guid Value);
