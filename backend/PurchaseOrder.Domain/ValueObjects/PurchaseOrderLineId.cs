namespace PurchaseOrder.Domain.ValueObjects;

/// <summary>
/// Strongly typed identifier for a purchase order line.
/// </summary>
public readonly record struct PurchaseOrderLineId(Guid Value);
