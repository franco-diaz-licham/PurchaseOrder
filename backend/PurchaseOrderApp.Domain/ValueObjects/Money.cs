using PurchaseOrderApp.Domain.Core;

namespace PurchaseOrderApp.Domain.ValueObjects;

/// <summary>
/// Non-negative monetary amount used for item costs and committed value.
/// </summary>
public readonly record struct Money
{
    /// <summary>
    /// Creates a monetary amount from a non-negative value.
    /// </summary>
    public Money(decimal value)
    {
        if (value < 0) throw new DomainException("Money cannot be negative.");
        Value = value;
    }

    /// <summary>
    /// Numeric monetary value.
    /// </summary>
    public decimal Value { get; }

    /// <summary>
    /// Zero monetary amount.
    /// </summary>
    public static Money Zero => new(0);
}
