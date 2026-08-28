using PurchaseOrderApp.Domain.Entities;
using PurchaseOrderApp.Domain.ValueObjects;
using PurchaseOrderApp.Infrastructure.Repositories;
using PurchaseOrderApp.Tests.Shared;
using Shouldly;

namespace PurchaseOrderApp.Tests.Infrastructure;

[TestFixture]
public sealed class PurchaseOrderRepositoryTests : DatabaseFixture
{
    [Test]
    public async Task AddAsync_ShouldPersistPurchaseOrderWithGeneratedNumber()
    {
        // Arrange
        var warehouse = Warehouse.Create("NSW", "New South Wales", TestData.User, TestData.OccurredAt);
        var item = TestData.CreateUnitItem(standardCost: 2.50m);
        var purchaseOrder = PurchaseOrder.CreatePending(warehouse.Id, TestData.User, TestData.OccurredAt);
        purchaseOrder.AddLine(item, new Quantity(4), TestData.User, TestData.OccurredAt);

        await Db.AddRangeAsync(warehouse, item);
        var sut = new PurchaseOrderRepository(Db);

        // Act
        await sut.AddAsync(purchaseOrder, CancellationToken.None);
        await Db.SaveChangesAsync();

        Db.ChangeTracker.Clear();
        var result = await sut.GetResponseAsync(purchaseOrder.Id, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.PurchaseOrderNumber.ShouldStartWith("PO-");
        result.Status.ShouldBe("Pending");
        result.Lines.Count.ShouldBe(1);
        result.SubtotalAmount.ShouldBe(10.00m);
        result.GstAmount.ShouldBe(1.00m);
        result.TotalAmount.ShouldBe(11.00m);
    }
}
