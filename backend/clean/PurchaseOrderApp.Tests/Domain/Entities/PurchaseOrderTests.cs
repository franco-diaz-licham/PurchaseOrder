using PurchaseOrderApp.Domain.Core;
using PurchaseOrderApp.Domain.Enums;
using PurchaseOrderApp.Domain.ValueObjects;
using PurchaseOrderApp.Tests.Shared;
using Shouldly;

namespace PurchaseOrderApp.Tests.Domain.Entities;

[TestFixture]
public sealed class PurchaseOrderTests
{
    [Test]
    public void CreatePending_ShouldSetPendingStatusAndAuditFields()
    {
        // Arrange
        var warehouseId = TestData.WarehouseId;

        // Act
        var purchaseOrder = PurchaseOrderApp.Domain.Entities.PurchaseOrder.CreatePending(
            warehouseId,
            TestData.User,
            TestData.OccurredAt);

        // Assert
        purchaseOrder.Id.Value.ShouldNotBe(Guid.Empty);
        purchaseOrder.PurchaseOrderNumber.ShouldBeNull();
        purchaseOrder.WarehouseId.ShouldBe(warehouseId);
        purchaseOrder.Status.ShouldBe(PurchaseOrderStatus.Pending);
        purchaseOrder.CreatedBy.ShouldBe(TestData.User);
        purchaseOrder.CreatedAt.ShouldBe(TestData.OccurredAt);
        purchaseOrder.Lines.ShouldBeEmpty();
    }

    [Test]
    public void AddLine_ShouldAddOneLineWithOrderedQuantity()
    {
        // Arrange
        var purchaseOrder = TestData.CreatePendingPurchaseOrder();
        var item = TestData.CreateUnitItem();

        // Act
        var line = purchaseOrder.AddLine(item, new Quantity(5), TestData.User, TestData.OccurredAt);

        // Assert
        purchaseOrder.Lines.Single().ShouldBe(line);
        line.InventoryItemId.ShouldBe(item.Id);
        line.QuantityOrdered.Value.ShouldBe(5);
        line.QuantityReserved.ShouldBe(Quantity.Zero);
        line.QuantityRemaining.Value.ShouldBe(5);
        purchaseOrder.HasOutstandingLines.ShouldBeTrue();
    }

    [Test]
    public void AddLine_ShouldThrow_WhenSameItemIsAddedTwice()
    {
        // Arrange
        var purchaseOrder = TestData.CreatePendingPurchaseOrder();
        var item = TestData.CreateUnitItem();
        purchaseOrder.AddLine(item, new Quantity(5), TestData.User, TestData.OccurredAt);

        // Act
        var exception = Should.Throw<DomainException>(() =>
            purchaseOrder.AddLine(item, new Quantity(2), TestData.User, TestData.OccurredAt));

        // Assert
        exception.Message.ShouldBe("Inventory item has already been added to this purchase order.");
    }

    [Test]
    public void AddLine_ShouldThrow_WhenUnitTrackedQuantityIsDecimal()
    {
        // Arrange
        var purchaseOrder = TestData.CreatePendingPurchaseOrder();
        var item = TestData.CreateUnitItem();

        // Act
        var exception = Should.Throw<DomainException>(() =>
            purchaseOrder.AddLine(item, new Quantity(2.5m), TestData.User, TestData.OccurredAt));

        // Assert
        exception.Message.ShouldBe("Unit-tracked item quantities must be whole numbers.");
    }

    [Test]
    public void ReserveLine_ShouldThrow_WhenPurchaseOrderIsNotApproved()
    {
        // Arrange
        var purchaseOrder = TestData.CreatePendingPurchaseOrder();
        var item = TestData.CreateUnitItem();
        var line = purchaseOrder.AddLine(item, new Quantity(5), TestData.User, TestData.OccurredAt);

        // Act
        var exception = Should.Throw<DomainException>(() =>
            purchaseOrder.ReserveLine(line.Id, new Quantity(1), TestData.User, TestData.OccurredAt));

        // Assert
        exception.Message.ShouldBe("Only approved purchase orders can receive reservations.");
    }

    [Test]
    public void ReserveLine_ShouldReservePartOfTheLine_WhenPurchaseOrderIsApproved()
    {
        // Arrange
        var purchaseOrder = TestData.CreatePendingPurchaseOrder();
        var item = TestData.CreateUnitItem();
        var line = purchaseOrder.AddLine(item, new Quantity(5), TestData.User, TestData.OccurredAt);
        purchaseOrder.Approve(TestData.User, TestData.OccurredAt);

        // Act
        purchaseOrder.ReserveLine(line.Id, new Quantity(2), TestData.User, TestData.OccurredAt);

        // Assert
        line.QuantityReserved.Value.ShouldBe(2);
        line.QuantityRemaining.Value.ShouldBe(3);
        line.IsFullyReserved.ShouldBeFalse();
    }

    [Test]
    public void ReleaseLine_ShouldReduceReservedQuantity()
    {
        // Arrange
        var purchaseOrder = TestData.CreatePendingPurchaseOrder();
        var item = TestData.CreateUnitItem();
        var line = purchaseOrder.AddLine(item, new Quantity(5), TestData.User, TestData.OccurredAt);
        purchaseOrder.Approve(TestData.User, TestData.OccurredAt);
        purchaseOrder.ReserveLine(line.Id, new Quantity(4), TestData.User, TestData.OccurredAt);

        // Act
        purchaseOrder.ReleaseLine(line.Id, new Quantity(1), TestData.User, TestData.OccurredAt);

        // Assert
        line.QuantityReserved.Value.ShouldBe(3);
        line.QuantityRemaining.Value.ShouldBe(2);
    }

    [Test]
    public void ReleaseLine_ShouldThrow_WhenReleaseExceedsReservedQuantity()
    {
        // Arrange
        var purchaseOrder = TestData.CreatePendingPurchaseOrder();
        var item = TestData.CreateUnitItem();
        var line = purchaseOrder.AddLine(item, new Quantity(5), TestData.User, TestData.OccurredAt);
        purchaseOrder.Approve(TestData.User, TestData.OccurredAt);
        purchaseOrder.ReserveLine(line.Id, new Quantity(2), TestData.User, TestData.OccurredAt);

        // Act
        var exception = Should.Throw<DomainException>(() =>
            purchaseOrder.ReleaseLine(line.Id, new Quantity(3), TestData.User, TestData.OccurredAt));

        // Assert
        exception.Message.ShouldBe("Release quantity exceeds the purchase order line reserved quantity.");
    }

    [Test]
    public void RemoveLine_ShouldRemoveLine_WhenLineHasNoReservations()
    {
        // Arrange
        var purchaseOrder = TestData.CreatePendingPurchaseOrder();
        var item = TestData.CreateUnitItem();
        var line = purchaseOrder.AddLine(item, new Quantity(5), TestData.User, TestData.OccurredAt);

        // Act
        purchaseOrder.RemoveLine(line.Id, TestData.User, TestData.OccurredAt);

        // Assert
        purchaseOrder.Lines.ShouldBeEmpty();
    }

    [Test]
    public void RemoveLine_ShouldThrow_WhenLineHasReservations()
    {
        // Arrange
        var purchaseOrder = TestData.CreatePendingPurchaseOrder();
        var item = TestData.CreateUnitItem();
        var line = purchaseOrder.AddLine(item, new Quantity(5), TestData.User, TestData.OccurredAt);
        purchaseOrder.Approve(TestData.User, TestData.OccurredAt);
        purchaseOrder.ReserveLine(line.Id, new Quantity(1), TestData.User, TestData.OccurredAt);

        // Act
        var exception = Should.Throw<DomainException>(() =>
            purchaseOrder.RemoveLine(line.Id, TestData.User, TestData.OccurredAt));

        // Assert
        exception.Message.ShouldBe("Purchase order line reservations must be released before the line can be removed.");
    }

    [Test]
    public void UpdateLineQuantity_ShouldChangeOrderedAndRemainingQuantity()
    {
        // Arrange
        var purchaseOrder = TestData.CreatePendingPurchaseOrder();
        var item = TestData.CreateUnitItem();
        var line = purchaseOrder.AddLine(item, new Quantity(5), TestData.User, TestData.OccurredAt);

        // Act
        purchaseOrder.UpdateLineQuantity(line.Id, new Quantity(8), TestData.User, TestData.OccurredAt);

        // Assert
        line.QuantityOrdered.Value.ShouldBe(8);
        line.QuantityRemaining.Value.ShouldBe(8);
        purchaseOrder.UpdatedBy.ShouldBe(TestData.User);
    }

    [Test]
    public void UpdateLineQuantity_ShouldThrow_WhenQuantityIsLessThanReservedQuantity()
    {
        // Arrange
        var purchaseOrder = TestData.CreatePendingPurchaseOrder();
        var item = TestData.CreateUnitItem();
        var line = purchaseOrder.AddLine(item, new Quantity(5), TestData.User, TestData.OccurredAt);
        purchaseOrder.Approve(TestData.User, TestData.OccurredAt);
        purchaseOrder.ReserveLine(line.Id, new Quantity(3), TestData.User, TestData.OccurredAt);

        // Act
        var exception = Should.Throw<DomainException>(() =>
            purchaseOrder.UpdateLineQuantity(line.Id, new Quantity(2), TestData.User, TestData.OccurredAt));

        // Assert
        exception.Message.ShouldBe("Purchase order line quantity cannot be less than the reserved quantity.");
    }

    [Test]
    public void ReserveLine_ShouldThrow_WhenReservationExceedsRemainingQuantity()
    {
        // Arrange
        var purchaseOrder = TestData.CreatePendingPurchaseOrder();
        var item = TestData.CreateUnitItem();
        var line = purchaseOrder.AddLine(item, new Quantity(5), TestData.User, TestData.OccurredAt);
        purchaseOrder.Approve(TestData.User, TestData.OccurredAt);

        // Act
        var exception = Should.Throw<DomainException>(() =>
            purchaseOrder.ReserveLine(line.Id, new Quantity(6), TestData.User, TestData.OccurredAt));

        // Assert
        exception.Message.ShouldBe("Reservation quantity exceeds the purchase order line remaining quantity.");
    }

    [Test]
    public void Cancel_ShouldThrow_WhenAnyLineHasActiveReservations()
    {
        // Arrange
        var purchaseOrder = TestData.CreatePendingPurchaseOrder();
        var item = TestData.CreateUnitItem();
        var line = purchaseOrder.AddLine(item, new Quantity(5), TestData.User, TestData.OccurredAt);
        purchaseOrder.Approve(TestData.User, TestData.OccurredAt);
        purchaseOrder.ReserveLine(line.Id, new Quantity(2), TestData.User, TestData.OccurredAt);

        // Act
        var exception = Should.Throw<DomainException>(() =>
            purchaseOrder.Cancel(TestData.User, TestData.OccurredAt));

        // Assert
        exception.Message.ShouldBe("Purchase orders with active reservations cannot be cancelled.");
    }

    [Test]
    public void Approve_ShouldSetStatusToApproved()
    {
        // Arrange
        var purchaseOrder = TestData.CreatePendingPurchaseOrder();

        // Act
        purchaseOrder.Approve(TestData.User, TestData.OccurredAt);

        // Assert
        purchaseOrder.Status.ShouldBe(PurchaseOrderStatus.Approved);
        purchaseOrder.UpdatedBy.ShouldBe(TestData.User);
    }

    [Test]
    public void Close_ShouldSetStatusToClosed()
    {
        // Arrange
        var purchaseOrder = TestData.CreatePendingPurchaseOrder();

        // Act
        purchaseOrder.Close(TestData.User, TestData.OccurredAt);

        // Assert
        purchaseOrder.Status.ShouldBe(PurchaseOrderStatus.Closed);
        purchaseOrder.UpdatedBy.ShouldBe(TestData.User);
    }

    [Test]
    public void Cancel_ShouldSetStatusToCancelled_WhenThereAreNoReservations()
    {
        // Arrange
        var purchaseOrder = TestData.CreatePendingPurchaseOrder();

        // Act
        purchaseOrder.Cancel(TestData.User, TestData.OccurredAt);

        // Assert
        purchaseOrder.Status.ShouldBe(PurchaseOrderStatus.Cancelled);
        purchaseOrder.UpdatedBy.ShouldBe(TestData.User);
    }
}
