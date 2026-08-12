using PurchaseOrderApp.Domain.Enums;
using PurchaseOrderApp.Domain.Core;
using PurchaseOrderApp.Domain.ValueObjects;
using PurchaseOrderApp.Tests.Shared;
using Shouldly;

namespace PurchaseOrderApp.Tests.Domain.Entities;

[TestFixture]
public sealed class InventoryItemTests
{
    [Test]
    public void Create_ShouldTrimNamesAndSetStandardCost()
    {
        // Arrange
        const string sku = " bolt-10 ";
        const string name = " 10mm Bolt ";
        var standardCost = new Money(1.25m);

        // Act
        var item = PurchaseOrderApp.Domain.Entities.InventoryItem.Create(
            sku,
            name,
            InventoryItemCategory.Hardware,
            InventoryTrackingMode.Unit,
            standardCost,
            TestData.User,
            TestData.OccurredAt);

        // Assert
        item.Id.Value.ShouldNotBe(Guid.Empty);
        item.Sku.ShouldBe("bolt-10");
        item.Name.ShouldBe("10mm Bolt");
        item.Category.ShouldBe(InventoryItemCategory.Hardware);
        item.TrackingMode.ShouldBe(InventoryTrackingMode.Unit);
        item.StandardCost.Value.ShouldBe(1.25m);
    }

    [Test]
    public void ChangeStandardCost_ShouldUpdateCostAndAuditFields()
    {
        // Arrange
        var item = TestData.CreateUnitItem();

        // Act
        item.ChangeStandardCost(new Money(2.10m), TestData.User, TestData.OccurredAt);

        // Assert
        item.StandardCost.Value.ShouldBe(2.10m);
        item.UpdatedBy.ShouldBe(TestData.User);
        item.UpdatedAt.ShouldBe(TestData.OccurredAt);
    }

    [Test]
    public void EnsureValidQuantity_ShouldThrow_WhenUnitItemReceivesDecimalQuantity()
    {
        // Arrange
        var item = TestData.CreateUnitItem();

        // Act
        var exception = Should.Throw<DomainException>(() => item.EnsureValidQuantity(new Quantity(1.5m)));

        // Assert
        exception.Message.ShouldBe("Unit-tracked item quantities must be whole numbers.");
    }
}
