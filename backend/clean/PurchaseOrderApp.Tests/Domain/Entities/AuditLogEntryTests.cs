using PurchaseOrderApp.Domain.Core;
using PurchaseOrderApp.Domain.Entities;
using PurchaseOrderApp.Domain.Enums;
using PurchaseOrderApp.Domain.Events;
using PurchaseOrderApp.Domain.ValueObjects;
using PurchaseOrderApp.Tests.Shared;
using Shouldly;

namespace PurchaseOrderApp.Tests.Domain.Entities;

[TestFixture]
public sealed class AuditLogEntryTests
{
    [Test]
    public void RecordReservation_ShouldCreateReserveAuditEntry()
    {
        // Arrange
        var domainEvent = CreateReservedEvent(new Quantity(5), new Quantity(95));

        // Act
        var entry = AuditLogEntry.RecordReservation(domainEvent);

        // Assert
        entry.Id.Value.ShouldNotBe(Guid.Empty);
        entry.Action.ShouldBe(AuditAction.Reserve);
        entry.InventoryItemId.ShouldBe(domainEvent.InventoryItemId);
        entry.Quantity.Value.ShouldBe(5);
        entry.ResultingAvailableQuantity.Value.ShouldBe(95);
        entry.CreatedBy.ShouldBe(TestData.User);
    }

    [Test]
    public void RecordRelease_ShouldCreateReleaseAuditEntry()
    {
        // Arrange
        var domainEvent = new StockReleasedEvent(
            new StockReservationId(Guid.NewGuid()),
            new PurchaseOrderLineId(Guid.NewGuid()),
            TestData.WarehouseId,
            new InventoryItemId(Guid.NewGuid()),
            new Quantity(2.500m),
            new Quantity(97.500m),
            TestData.User,
            TestData.OccurredAt);

        // Act
        var entry = AuditLogEntry.RecordRelease(domainEvent);

        // Assert
        entry.Action.ShouldBe(AuditAction.Release);
        entry.StockReservationId.ShouldBe(domainEvent.StockReservationId);
        entry.Quantity.Value.ShouldBe(2.500m);
        entry.ResultingAvailableQuantity.Value.ShouldBe(97.500m);
    }

    [Test]
    public void RecordReservation_ShouldThrow_WhenQuantityIsZero()
    {
        // Arrange
        var domainEvent = CreateReservedEvent(Quantity.Zero, new Quantity(100));

        // Act
        var exception = Should.Throw<DomainException>(() => AuditLogEntry.RecordReservation(domainEvent));

        // Assert
        exception.Message.ShouldBe("Audit quantity must be greater than zero.");
    }

    private static StockReservedEvent CreateReservedEvent(Quantity quantity, Quantity resultingAvailableQuantity)
    {
        return new StockReservedEvent(
            new StockReservationId(Guid.NewGuid()),
            new PurchaseOrderLineId(Guid.NewGuid()),
            TestData.WarehouseId,
            new InventoryItemId(Guid.NewGuid()),
            quantity,
            resultingAvailableQuantity,
            TestData.User,
            TestData.OccurredAt);
    }
}
