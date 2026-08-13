using PurchaseOrderApp.Domain.Core;
using PurchaseOrderApp.Domain.Enums;

namespace PurchaseOrderApp.Domain.ValueObjects;

/// <summary>
/// Non-negative stock quantity with up to 3 decimal places of precision.
/// </summary>
public readonly record struct Quantity
{
    private const int MaximumDecimalPlaces = 3;

    /// <summary>
    /// Creates a quantity from a non-negative value with no more than 3 decimal places.
    /// </summary>
    public Quantity(decimal value)
    {
        if (value < 0) throw new DomainException("Quantity cannot be negative.");
        if (decimal.Round(value, MaximumDecimalPlaces) != value) throw new DomainException("Quantity cannot have more than 3 decimal places.");
        Value = value;
    }

    /// <summary>
    /// Numeric quantity value.
    /// </summary>
    public decimal Value { get; }

    /// <summary>
    /// Zero quantity.
    /// </summary>
    public static Quantity Zero => new(0);

    /// <summary>
    /// Indicates whether this quantity is zero.
    /// </summary>
    public bool IsZero => Value == 0;

    /// <summary>
    /// Ensures the quantity is valid for the supplied inventory tracking mode.
    /// </summary>
    public void EnsureValidFor(InventoryTrackingMode trackingMode)
    {
        if (trackingMode == InventoryTrackingMode.Unit && decimal.Truncate(Value) != Value) throw new DomainException("Unit-tracked item quantities must be whole numbers.");
    }

    /// <summary>
    /// Returns the sum of this quantity and another quantity.
    /// </summary>
    public Quantity Add(Quantity quantity) => new(Value + quantity.Value);

    /// <summary>
    /// Returns this quantity reduced by another quantity.
    /// </summary>
    public Quantity Subtract(Quantity quantity)
    {
        if (Value < quantity.Value) throw new DomainException("Quantity cannot be reduced below zero.");
        return new Quantity(Value - quantity.Value);
    }
}
