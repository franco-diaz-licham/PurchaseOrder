using PurchaseOrderApp.Domain.Entities;
using PurchaseOrderApp.Domain.Services;
using PurchaseOrderApp.Domain.ValueObjects;
using PurchaseOrderApp.Infrastructure.Repositories;
using PurchaseOrderApp.Tests.Shared;
using Shouldly;

namespace PurchaseOrderApp.Tests.Infrastructure;

[TestFixture]
public sealed class FinanceRepositoryTests : DatabaseFixture
{
    [Test]
    public async Task ListWarehouseCommittedStockValuesAsync_ShouldUseReservationCostSnapshot()
    {
        // Arrange
        var warehouse = Warehouse.Create("QLD", "Queensland", TestData.User, TestData.OccurredAt);
        var item = TestData.CreateWeightItem(standardCost: 1.75m);
        var stock = TestData.CreateWarehouseStock(warehouse.Id, item.Id, onHandQuantity: 35);
        var purchaseOrder = PurchaseOrder.CreatePending(warehouse.Id, TestData.User, TestData.OccurredAt);
        var line = purchaseOrder.AddLine(item, new Quantity(10.500m), TestData.User, TestData.OccurredAt);
        purchaseOrder.Approve(TestData.User, TestData.OccurredAt);

        var reservation = StockReservationDomainService.Reserve(
            purchaseOrder,
            line.Id,
            stock,
            item,
            Quantity.Zero,
            new Quantity(5.500m),
            TestData.User,
            TestData.OccurredAt);

        item.ChangeStandardCost(new Money(99.99m), TestData.User, TestData.OccurredAt.AddMinutes(5));

        await Db.AddRangeAsync(warehouse, item, stock, purchaseOrder, reservation);
        await Db.SaveChangesAsync();
        Db.ChangeTracker.Clear();
        var sut = new FinanceRepository(Db);

        // Act
        var result = await sut.ListWarehouseCommittedStockValuesAsync(CancellationToken.None);

        // Assert
        var warehouseValue = result.Single();
        warehouseValue.WarehouseId.ShouldBe(warehouse.Id.Value);
        warehouseValue.ReservationCount.ShouldBe(1);
        warehouseValue.ReservedQuantity.ShouldBe(5.500m);
        warehouseValue.CommittedValue.ShouldBe(9.625m);
        warehouseValue.Reservations.Single().UnitCostSnapshot.ShouldBe(1.75m);
        warehouseValue.Reservations.Single().CommittedValue.ShouldBe(9.625m);
    }
}
