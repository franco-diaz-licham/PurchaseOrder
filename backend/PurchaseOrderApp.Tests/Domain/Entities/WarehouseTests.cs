using PurchaseOrderApp.Domain.Core;
using PurchaseOrderApp.Tests.Shared;
using Shouldly;

namespace PurchaseOrderApp.Tests.Domain.Entities;

[TestFixture]
public sealed class WarehouseTests
{
    [Test]
    public void Create_ShouldTrimNameAndUppercaseCode()
    {
        // Arrange
        const string code = " syd ";
        const string name = " Sydney Fulfilment Centre ";

        // Act
        var warehouse = PurchaseOrderApp.Domain.Entities.Warehouse.Create(
            code,
            name,
            TestData.User,
            TestData.OccurredAt);

        // Assert
        warehouse.Id.Value.ShouldNotBe(Guid.Empty);
        warehouse.Code.ShouldBe("SYD");
        warehouse.Name.ShouldBe("Sydney Fulfilment Centre");
        warehouse.CreatedBy.ShouldBe(TestData.User);
    }

    [Test]
    public void Rename_ShouldUpdateNameAndAuditFields()
    {
        // Arrange
        var warehouse = PurchaseOrderApp.Domain.Entities.Warehouse.Create("SYD", "Sydney", TestData.User, TestData.OccurredAt);

        // Act
        warehouse.Rename(" Sydney Fulfilment Centre ", TestData.User, TestData.OccurredAt);

        // Assert
        warehouse.Name.ShouldBe("Sydney Fulfilment Centre");
        warehouse.UpdatedBy.ShouldBe(TestData.User);
        warehouse.UpdatedAt.ShouldBe(TestData.OccurredAt);
    }

    [Test]
    public void Rename_ShouldThrow_WhenNameIsMissing()
    {
        // Arrange
        var warehouse = PurchaseOrderApp.Domain.Entities.Warehouse.Create("SYD", "Sydney", TestData.User, TestData.OccurredAt);

        // Act
        var exception = Should.Throw<DomainException>(() => warehouse.Rename(" ", TestData.User, TestData.OccurredAt));

        // Assert
        exception.Message.ShouldBe("name is required.");
    }
}
