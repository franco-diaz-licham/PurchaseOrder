using PurchaseOrderApp.Domain.Core;
using PurchaseOrderApp.Domain.Enums;
using PurchaseOrderApp.Domain.ValueObjects;
using Shouldly;

namespace PurchaseOrderApp.Tests.Domain.ValueObjects;

[TestFixture]
public sealed class QuantityTests
{
    [Test]
    public void Constructor_ShouldThrow_WhenQuantityIsNegative()
    {
        // Act
        var exception = Should.Throw<DomainException>(() => new Quantity(-1));

        // Assert
        exception.Message.ShouldBe("Quantity cannot be negative.");
    }

    [Test]
    public void Constructor_ShouldThrow_WhenQuantityHasMoreThanThreeDecimalPlaces()
    {
        // Act
        var exception = Should.Throw<DomainException>(() => new Quantity(1.1234m));

        // Assert
        exception.Message.ShouldBe("Quantity cannot have more than 3 decimal places.");
    }

    [Test]
    public void EnsureValidFor_ShouldAllowDecimalQuantitiesForWeightTrackedItems()
    {
        // Arrange
        var quantity = new Quantity(10.500m);

        // Act
        Should.NotThrow(() => quantity.EnsureValidFor(InventoryTrackingMode.Weight));
    }

    [Test]
    public void EnsureValidFor_ShouldRejectDecimalQuantitiesForUnitTrackedItems()
    {
        // Arrange
        var quantity = new Quantity(10.500m);

        // Act
        var exception = Should.Throw<DomainException>(() => quantity.EnsureValidFor(InventoryTrackingMode.Unit));

        // Assert
        exception.Message.ShouldBe("Unit-tracked item quantities must be whole numbers.");
    }

    [Test]
    public void Add_ShouldReturnCombinedQuantity()
    {
        // Act
        var quantity = new Quantity(2.25m).Add(new Quantity(3.125m));

        // Assert
        quantity.Value.ShouldBe(5.375m);
    }

    [Test]
    public void Subtract_ShouldReturnReducedQuantity()
    {
        // Act
        var quantity = new Quantity(5.375m).Subtract(new Quantity(3.125m));

        // Assert
        quantity.Value.ShouldBe(2.250m);
    }

    [Test]
    public void Subtract_ShouldThrow_WhenResultWouldBeNegative()
    {
        // Act
        var exception = Should.Throw<DomainException>(() => new Quantity(1).Subtract(new Quantity(2)));

        // Assert
        exception.Message.ShouldBe("Quantity cannot be reduced below zero.");
    }
}
