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
        const string sku = " shackle-12t ";
        const string name = " 12t Bow Shackle ";
        var standardCost = new Money(48m);

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
        item.Sku.ShouldBe("shackle-12t");
        item.Name.ShouldBe("12t Bow Shackle");
        item.Category.ShouldBe(InventoryItemCategory.Hardware);
        item.TrackingMode.ShouldBe(InventoryTrackingMode.Unit);
        item.StandardCost.Value.ShouldBe(48m);
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
