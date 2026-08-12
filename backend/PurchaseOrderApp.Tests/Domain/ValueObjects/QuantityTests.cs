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
        // Arrange
        const decimal value = -1m;

        // Act
        var exception = Should.Throw<DomainException>(() => new Quantity(value));

        // Assert
        exception.Message.ShouldBe("Quantity cannot be negative.");
    }

    [Test]
    public void Constructor_ShouldThrow_WhenQuantityHasMoreThanThreeDecimalPlaces()
    {
        // Arrange
        const decimal value = 1.1234m;

        // Act
        var exception = Should.Throw<DomainException>(() => new Quantity(value));

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

        // Assert
        quantity.Value.ShouldBe(10.500m);
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
        // Arrange
        var firstQuantity = new Quantity(2.25m);
        var secondQuantity = new Quantity(3.125m);

        // Act
        var quantity = firstQuantity.Add(secondQuantity);

        // Assert
        quantity.Value.ShouldBe(5.375m);
    }

    [Test]
    public void Subtract_ShouldReturnReducedQuantity()
    {
        // Arrange
        var firstQuantity = new Quantity(5.375m);
        var secondQuantity = new Quantity(3.125m);

        // Act
        var quantity = firstQuantity.Subtract(secondQuantity);

        // Assert
        quantity.Value.ShouldBe(2.250m);
    }

    [Test]
    public void Subtract_ShouldThrow_WhenResultWouldBeNegative()
    {
        // Arrange
        var firstQuantity = new Quantity(1);
        var secondQuantity = new Quantity(2);

        // Act
        var exception = Should.Throw<DomainException>(() => firstQuantity.Subtract(secondQuantity));

        // Assert
        exception.Message.ShouldBe("Quantity cannot be reduced below zero.");
    }
}
