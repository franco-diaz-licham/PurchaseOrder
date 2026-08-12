using PurchaseOrderApp.Domain.ValueObjects;
using Shouldly;

namespace PurchaseOrderApp.Tests.Domain.ValueObjects;

[TestFixture]
public sealed class IdentifierTests
{
    [Test]
    public void IdentifierValueObjects_ShouldExposeWrappedGuid()
    {
        // Arrange
        var id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        // Act
        var auditLogEntryId = new AuditLogEntryId(id);
        var inventoryItemId = new InventoryItemId(id);
        var purchaseOrderId = new PurchaseOrderId(id);
        var purchaseOrderLineId = new PurchaseOrderLineId(id);
        var stockReservationId = new StockReservationId(id);
        var warehouseId = new WarehouseId(id);
        var warehouseStockId = new WarehouseStockId(id);

        // Assert
        auditLogEntryId.Value.ShouldBe(id);
        inventoryItemId.Value.ShouldBe(id);
        purchaseOrderId.Value.ShouldBe(id);
        purchaseOrderLineId.Value.ShouldBe(id);
        stockReservationId.Value.ShouldBe(id);
        warehouseId.Value.ShouldBe(id);
        warehouseStockId.Value.ShouldBe(id);
    }

    [Test]
    public void IdentifierValueObjects_ShouldUseValueEquality_WhenWrappedGuidMatches()
    {
        // Arrange
        var id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

        // Act
        var first = new PurchaseOrderId(id);
        var second = new PurchaseOrderId(id);

        // Assert
        first.ShouldBe(second);
        first.GetHashCode().ShouldBe(second.GetHashCode());
    }
}
