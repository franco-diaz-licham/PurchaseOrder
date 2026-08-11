using PurchaseOrder.Domain.Core;

namespace PurchaseOrder.Domain.ValueObjects;

/// <summary>
/// Non-negative monetary amount used for item costs and committed value.
/// </summary>
public readonly record struct Money
{
    public Money(decimal value)
    {
        if (value < 0) throw new DomainException("Money cannot be negative.");
        Value = value;
    }

    public decimal Value { get; }

    public static Money Zero => new(0);

    public static Money operator *(Money money, Quantity quantity) => new(money.Value * quantity.Value);
}
