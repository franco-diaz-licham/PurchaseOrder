using PurchaseOrderApp.Domain.Core;
using PurchaseOrderApp.Domain.ValueObjects;
using PurchaseOrderApp.Tests.Shared;
using Shouldly;

namespace PurchaseOrderApp.Tests.Domain.Entities;

[TestFixture]
public sealed class WarehouseStockTests
{
    [Test]
    public void EnsureCanReserve_ShouldAllowReservation_WhenRequestedQuantityIsAvailable()
    {
        // Arrange
        var stock = TestData.CreateWarehouseStock(onHandQuantity: 100);

        // Act
        Should.NotThrow(() => stock.EnsureCanReserve(new Quantity(25), new Quantity(75)));
    }

    [Test]
    public void EnsureCanReserve_ShouldThrow_WhenRequestedQuantityExceedsAvailableQuantity()
    {
        // Arrange
        var stock = TestData.CreateWarehouseStock(onHandQuantity: 100);

        // Act
        var exception = Should.Throw<DomainException>(() => stock.EnsureCanReserve(new Quantity(25), new Quantity(76)));

        // Assert
        exception.Message.ShouldBe("Reservation quantity exceeds available stock.");
    }

    [Test]
    public void CalculateAvailableQuantity_ShouldSubtractActiveReservationsFromOnHandQuantity()
    {
        // Arrange
        var stock = TestData.CreateWarehouseStock(onHandQuantity: 100);

        // Act
        var availableQuantity = stock.CalculateAvailableQuantity(new Quantity(37.500m));

        // Assert
        availableQuantity.Value.ShouldBe(62.500m);
    }
}
