using PurchaseOrderApp.Domain.Entities;
using PurchaseOrderApp.Domain.Services;
using PurchaseOrderApp.Domain.ValueObjects;
using PurchaseOrderApp.Infrastructure.Repositories;
using PurchaseOrderApp.Tests.Shared;
using Shouldly;

namespace PurchaseOrderApp.Tests.Infrastructure;

[TestFixture]
public sealed class WarehouseStockRepositoryTests : DatabaseFixture
{
    [Test]
    public async Task ListResponsesAsync_ShouldReturnStockForWarehouseOnly()
    {
        // Arrange
        var warehouse = Warehouse.Create("SYD", "Sydney", TestData.User, TestData.OccurredAt);
        var otherWarehouse = Warehouse.Create("MEL", "Melbourne", TestData.User, TestData.OccurredAt);
        var item = TestData.CreateUnitItem();
        var warehouseStock = TestData.CreateWarehouseStock(warehouse.Id, item.Id, onHandQuantity: 35);
        var otherStock = TestData.CreateWarehouseStock(otherWarehouse.Id, item.Id, onHandQuantity: 50);
        var purchaseOrder = PurchaseOrder.CreatePending(warehouse.Id, TestData.User, TestData.OccurredAt);
        purchaseOrder.AddLine(item, new Quantity(20), TestData.User, TestData.OccurredAt);
        purchaseOrder.Approve(TestData.User, TestData.OccurredAt);
        var reservation = StockReservationDomainService.Reserve(
            purchaseOrder,
            purchaseOrder.Lines.Single().Id,
            warehouseStock,
            item,
            Quantity.Zero,
            new Quantity(12),
            TestData.User,
            TestData.OccurredAt);

        await Db.AddRangeAsync(warehouse, otherWarehouse, item, warehouseStock, otherStock, purchaseOrder, reservation);
        await Db.SaveChangesAsync();
        var sut = new WarehouseStockRepository(Db);

        // Act
        var result = await sut.ListResponsesAsync(warehouse.Id, CancellationToken.None);

        // Assert
        result.Count.ShouldBe(1);
        result.Single().WarehouseId.ShouldBe(warehouse.Id.Value);
        result.Single().InventoryItemId.ShouldBe(item.Id.Value);
        result.Single().OnHandQuantity.ShouldBe(35);
        result.Single().ActiveReservedQuantity.ShouldBe(12);
        result.Single().AvailableQuantity.ShouldBe(23);
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

    [Test]
    public async Task GetForUpdateAsync_ShouldHoldRowLockUntilTransactionCommits()
    {
        // Arrange
        var seed = await PurchaseOrderScenarioSeeder.SeedApprovedLineAsync(Db, onHandQuantity: 10m);
        await using var firstContext = CreateDatabaseContext();
        await using var firstTransaction = await firstContext.Database.BeginTransactionAsync();
        var firstRepository = new WarehouseStockRepository(firstContext);

        var firstLock = await firstRepository.GetForUpdateAsync(seed.WarehouseId, seed.InventoryItemId, CancellationToken.None);
        firstLock.ShouldNotBeNull();

        var secondLockStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var secondLockTask = Task.Run(async () => {
            await using var secondContext = CreateDatabaseContext();
            await using var secondTransaction = await secondContext.Database.BeginTransactionAsync();
            var secondRepository = new WarehouseStockRepository(secondContext);

            secondLockStarted.SetResult();
            var secondLock = await secondRepository.GetForUpdateAsync(seed.WarehouseId, seed.InventoryItemId, CancellationToken.None);
            await secondTransaction.CommitAsync();
            return secondLock;
        });

        await secondLockStarted.Task;
        await Task.Delay(250);

        // Act
        var secondLockWasStillWaiting = !secondLockTask.IsCompleted;
        await firstTransaction.CommitAsync();
        var result = await secondLockTask.WaitAsync(TimeSpan.FromSeconds(5));

        // Assert
        secondLockWasStillWaiting.ShouldBeTrue();
        result.ShouldNotBeNull();
        result.Id.ShouldBe(seed.WarehouseStockId);
    }

    [Test]
    public async Task GetForUpdateAsync_ShouldNotBlockDifferentItemInSameWarehouse()
    {
        // Arrange
        var seed = await PurchaseOrderScenarioSeeder.SeedTwoItemsInOneWarehouseAsync(Db);
        await using var firstContext = CreateDatabaseContext();
        await using var firstTransaction = await firstContext.Database.BeginTransactionAsync();
        var firstRepository = new WarehouseStockRepository(firstContext);

        var firstLock = await firstRepository.GetForUpdateAsync(seed.WarehouseId, seed.FirstInventoryItemId, CancellationToken.None);
        firstLock.ShouldNotBeNull();

        // Act
        await using var secondContext = CreateDatabaseContext();
        await using var secondTransaction = await secondContext.Database.BeginTransactionAsync();
        var secondRepository = new WarehouseStockRepository(secondContext);
        var secondLock = await secondRepository
            .GetForUpdateAsync(seed.WarehouseId, seed.SecondInventoryItemId, CancellationToken.None)
            .WaitAsync(TimeSpan.FromSeconds(5));

        // Assert
        secondLock.ShouldNotBeNull();
        secondLock.Id.ShouldBe(seed.SecondWarehouseStockId);

        await secondTransaction.CommitAsync();
        await firstTransaction.CommitAsync();
    }

    [Test]
    public async Task GetForUpdateAsync_ShouldNotBlockSameItemInDifferentWarehouse()
    {
        // Arrange
        var seed = await PurchaseOrderScenarioSeeder.SeedSameItemInTwoWarehousesAsync(Db);
        await using var firstContext = CreateDatabaseContext();
        await using var firstTransaction = await firstContext.Database.BeginTransactionAsync();
        var firstRepository = new WarehouseStockRepository(firstContext);

        var firstLock = await firstRepository.GetForUpdateAsync(seed.FirstWarehouseId, seed.InventoryItemId, CancellationToken.None);
        firstLock.ShouldNotBeNull();

        // Act
        await using var secondContext = CreateDatabaseContext();
        await using var secondTransaction = await secondContext.Database.BeginTransactionAsync();
        var secondRepository = new WarehouseStockRepository(secondContext);
        var secondLock = await secondRepository
            .GetForUpdateAsync(seed.SecondWarehouseId, seed.InventoryItemId, CancellationToken.None)
            .WaitAsync(TimeSpan.FromSeconds(5));

        // Assert
        secondLock.ShouldNotBeNull();
        secondLock.Id.ShouldBe(seed.SecondWarehouseStockId);

        await secondTransaction.CommitAsync();
        await firstTransaction.CommitAsync();
    }

    [Test]
    public async Task GetForUpdateAsync_ShouldCancelWaitingLockRequest()
    {
        // Arrange
        var seed = await PurchaseOrderScenarioSeeder.SeedApprovedLineAsync(Db, onHandQuantity: 10m);
        await using var firstContext = CreateDatabaseContext();
        await using var firstTransaction = await firstContext.Database.BeginTransactionAsync();
        var firstRepository = new WarehouseStockRepository(firstContext);

        var firstLock = await firstRepository.GetForUpdateAsync(seed.WarehouseId, seed.InventoryItemId, CancellationToken.None);
        firstLock.ShouldNotBeNull();

        await using var secondContext = CreateDatabaseContext();
        await using var secondTransaction = await secondContext.Database.BeginTransactionAsync();
        var secondRepository = new WarehouseStockRepository(secondContext);
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(250));

        try {
            // Act
            var exception = await Should.ThrowAsync<OperationCanceledException>(async () => await secondRepository.GetForUpdateAsync(seed.WarehouseId, seed.InventoryItemId, cancellation.Token));

            // Assert
            exception.ShouldNotBeNull();
        } finally {
            await secondTransaction.RollbackAsync();
            await firstTransaction.CommitAsync();
        }
    }
}
