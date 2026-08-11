using PurchaseOrder.Domain.Core;
using PurchaseOrder.Domain.Enums;

namespace PurchaseOrder.Domain.ValueObjects;

/// <summary>
/// Non-negative stock quantity with up to 3 decimal places of precision.
/// </summary>
public readonly record struct Quantity
{
    private const int MaximumDecimalPlaces = 3;

    public Quantity(decimal value)
    {
        if (value < 0) throw new DomainException("Quantity cannot be negative.");
        if (decimal.Round(value, MaximumDecimalPlaces) != value) throw new DomainException("Quantity cannot have more than 3 decimal places.");
        Value = value;
    }

    public decimal Value { get; }

    public static Quantity Zero => new(0);

    public bool IsZero => Value == 0;

    public void EnsureValidFor(InventoryTrackingMode trackingMode)
    {
        if (trackingMode == InventoryTrackingMode.Unit && decimal.Truncate(Value) != Value) throw new DomainException("Unit-tracked item quantities must be whole numbers.");
    }

    public Quantity Add(Quantity quantity) => new(Value + quantity.Value);

    public Quantity Subtract(Quantity quantity)
    {
        if (Value < quantity.Value) throw new DomainException("Quantity cannot be reduced below zero.");
        return new Quantity(Value - quantity.Value);
    }
}
