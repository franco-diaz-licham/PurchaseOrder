namespace PurchaseOrder.Domain.ValueObjects;

/// <summary>
/// Strongly typed identifier for a purchase order.
/// </summary>
public readonly record struct PurchaseOrderId(Guid Value);
