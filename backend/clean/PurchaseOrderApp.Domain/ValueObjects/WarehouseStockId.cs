namespace PurchaseOrderApp.Domain.ValueObjects;

/// <summary>
/// Strongly typed identifier for a warehouse stock balance.
/// </summary>
public readonly record struct WarehouseStockId(Guid Value);
