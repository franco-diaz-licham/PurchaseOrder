namespace PurchaseOrder.Domain.ValueObjects;

/// <summary>
/// Strongly typed identifier for a warehouse.
/// </summary>
public readonly record struct WarehouseId(Guid Value);
