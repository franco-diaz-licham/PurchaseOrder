using PurchaseOrderApp.Domain.Core;
using PurchaseOrderApp.Domain.Enums;
using PurchaseOrderApp.Domain.Events;
using PurchaseOrderApp.Domain.Services;
using PurchaseOrderApp.Domain.ValueObjects;
using PurchaseOrderApp.Tests.Shared;
using Shouldly;

namespace PurchaseOrderApp.Tests.Domain;

[TestFixture]
public sealed class StockReservationTests
{
    [Test]
    public void Reserve_ShouldCreateActiveReservationAndRaiseStockReservedEvent()
    {
        // Arrange
        var purchaseOrder = CreateApprovedPurchaseOrderWithLine(out var line);
        var item = line.InventoryItem;
        var stock = TestData.CreateWarehouseStock(purchaseOrder.WarehouseId, item.Id, onHandQuantity: 100);

        // Act
        var reservation = StockReservationDomainService.Reserve(
            purchaseOrder,
            line.Id,
            stock,
            item,
            Quantity.Zero,
            new Quantity(10),
            TestData.User,
            TestData.OccurredAt);

        // Assert
        reservation.Id.Value.ShouldNotBe(Guid.Empty);
        reservation.PurchaseOrderLineId.ShouldBe(line.Id);
        reservation.WarehouseId.ShouldBe(stock.WarehouseId);
        reservation.InventoryItemId.ShouldBe(item.Id);
        reservation.QuantityReserved.Value.ShouldBe(10);
        reservation.UnitCostSnapshot.ShouldBe(item.StandardCost);
        reservation.Status.ShouldBe(ReservationStatus.Active);
        reservation.CommittedValue.Value.ShouldBe(12.50m);
        reservation.DomainEvents.OfType<StockReservedEvent>().Single().ResultingAvailableQuantity.Value.ShouldBe(90);
        line.QuantityReserved.Value.ShouldBe(10);
    }

    [Test]
    public void Reserve_ShouldThrow_WhenWarehouseStockIsNotAvailable()
    {
        // Arrange
        var purchaseOrder = CreateApprovedPurchaseOrderWithLine(out var line);
        var item = line.InventoryItem;
        var stock = TestData.CreateWarehouseStock(purchaseOrder.WarehouseId, item.Id, onHandQuantity: 10);

        // Act
        var exception = Should.Throw<DomainException>(() => StockReservationDomainService.Reserve(
            purchaseOrder,
            line.Id,
            stock,
            item,
            new Quantity(8),
            new Quantity(3),
            TestData.User,
            TestData.OccurredAt));

        // Assert
        exception.Message.ShouldBe("Reservation quantity exceeds available stock.");
    }

    [Test]
    public void Release_ShouldReduceReservationAndRaiseStockReleasedEvent()
    {
        // Arrange
        var purchaseOrder = CreateApprovedPurchaseOrderWithLine(out var line);
        var item = line.InventoryItem;
        var stock = TestData.CreateWarehouseStock(purchaseOrder.WarehouseId, item.Id, onHandQuantity: 100);
        var reservation = StockReservationDomainService.Reserve(
            purchaseOrder,
            line.Id,
            stock,
            item,
            Quantity.Zero,
            new Quantity(10),
            TestData.User,
            TestData.OccurredAt);
        reservation.ClearDomainEvents();

        // Act
        StockReservationDomainService.Release(
            purchaseOrder,
            reservation,
            stock,
            new Quantity(10),
            new Quantity(4),
            TestData.User,
            TestData.OccurredAt);

        // Assert
        reservation.QuantityReserved.Value.ShouldBe(6);
        reservation.Status.ShouldBe(ReservationStatus.Active);
        reservation.DomainEvents.OfType<StockReleasedEvent>().Single().ResultingAvailableQuantity.Value.ShouldBe(94);
        line.QuantityReserved.Value.ShouldBe(6);
    }

    [Test]
    public void Release_ShouldMarkReservationAsReleased_WhenFullQuantityIsReleased()
    {
        // Arrange
        var purchaseOrder = CreateApprovedPurchaseOrderWithLine(out var line);
        var item = line.InventoryItem;
        var stock = TestData.CreateWarehouseStock(purchaseOrder.WarehouseId, item.Id, onHandQuantity: 100);
        var reservation = StockReservationDomainService.Reserve(
            purchaseOrder,
            line.Id,
            stock,
            item,
            Quantity.Zero,
            new Quantity(10),
            TestData.User,
            TestData.OccurredAt);

        // Act
        StockReservationDomainService.Release(
            purchaseOrder,
            reservation,
            stock,
            new Quantity(10),
            new Quantity(10),
            TestData.User,
            TestData.OccurredAt);

        // Assert
        reservation.QuantityReserved.ShouldBe(Quantity.Zero);
        reservation.Status.ShouldBe(ReservationStatus.Released);
        line.QuantityReserved.ShouldBe(Quantity.Zero);
    }

    [Test]
    public void Release_ShouldThrow_WhenReleaseQuantityExceedsReservation()
    {
        // Arrange
        var purchaseOrder = CreateApprovedPurchaseOrderWithLine(out var line);
        var item = line.InventoryItem;
        var stock = TestData.CreateWarehouseStock(purchaseOrder.WarehouseId, item.Id, onHandQuantity: 100);
        var reservation = StockReservationDomainService.Reserve(
            purchaseOrder,
            line.Id,
            stock,
            item,
            Quantity.Zero,
            new Quantity(10),
            TestData.User,
            TestData.OccurredAt);

        // Act
        var exception = Should.Throw<DomainException>(() => StockReservationDomainService.Release(
            purchaseOrder,
            reservation,
            stock,
            new Quantity(10),
            new Quantity(11),
            TestData.User,
            TestData.OccurredAt));

        // Assert
        exception.Message.ShouldBe("Quantity cannot be reduced below zero.");
    }

    private static PurchaseOrderApp.Domain.Entities.PurchaseOrder CreateApprovedPurchaseOrderWithLine(out PurchaseOrderApp.Domain.Entities.PurchaseOrderLine line)
    {
        var purchaseOrder = TestData.CreatePendingPurchaseOrder();
        var item = TestData.CreateUnitItem(standardCost: 1.25m);
        line = purchaseOrder.AddLine(item, new Quantity(20), TestData.User, TestData.OccurredAt);
        purchaseOrder.Approve(TestData.User, TestData.OccurredAt);
        return purchaseOrder;
    }
}
