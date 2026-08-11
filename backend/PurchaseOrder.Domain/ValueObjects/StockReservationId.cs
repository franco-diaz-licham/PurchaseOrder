namespace PurchaseOrder.Domain.ValueObjects;

/// <summary>
/// Strongly typed identifier for a stock reservation.
/// </summary>
public readonly record struct StockReservationId(Guid Value);
