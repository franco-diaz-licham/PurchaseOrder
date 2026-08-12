using PurchaseOrderApp.Domain.Entities;
using PurchaseOrderApp.Infrastructure.Repositories;
using PurchaseOrderApp.Tests.Shared;
using Shouldly;

namespace PurchaseOrderApp.Tests.Infrastructure;

[TestFixture]
public sealed class WarehouseStockRepositoryTests : DatabaseFixture
{
    [Test]
    public async Task ListAsync_ShouldReturnStockForWarehouseOnly()
    {
        // Arrange
        var warehouse = Warehouse.Create("SYD", "Sydney", TestData.User, TestData.OccurredAt);
        var otherWarehouse = Warehouse.Create("MEL", "Melbourne", TestData.User, TestData.OccurredAt);
        var item = TestData.CreateUnitItem();
        var warehouseStock = TestData.CreateWarehouseStock(warehouse.Id, item.Id, onHandQuantity: 35);
        var otherStock = TestData.CreateWarehouseStock(otherWarehouse.Id, item.Id, onHandQuantity: 50);

        await Db.AddRangeAsync(warehouse, otherWarehouse, item, warehouseStock, otherStock);
        await Db.SaveChangesAsync();
        var sut = new WarehouseStockRepository(Db);

        // Act
        var result = await sut.ListAsync(warehouse.Id, CancellationToken.None);

        // Assert
        result.Count.ShouldBe(1);
        result.Single().WarehouseId.ShouldBe(warehouse.Id);
        result.Single().OnHandQuantity.Value.ShouldBe(35);
    }

    [Test]
    public async Task GetForUpdateAsync_ShouldReturnWarehouseStock()
    {
        // Arrange
        var warehouse = Warehouse.Create("BNE", "Brisbane", TestData.User, TestData.OccurredAt);
        var item = TestData.CreateUnitItem();
        var warehouseStock = TestData.CreateWarehouseStock(warehouse.Id, item.Id, onHandQuantity: 42);

        await Db.AddRangeAsync(warehouse, item, warehouseStock);
        await Db.SaveChangesAsync();
        var sut = new WarehouseStockRepository(Db);

        // Act
        var result = await sut.GetForUpdateAsync(warehouse.Id, item.Id, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.WarehouseId.ShouldBe(warehouse.Id);
        result.InventoryItemId.ShouldBe(item.Id);
        result.OnHandQuantity.Value.ShouldBe(42);
    }
}
