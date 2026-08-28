using Microsoft.EntityFrameworkCore;
using PurchaseOrderApp.Domain.Entities;
using PurchaseOrderApp.Domain.Enums;
using PurchaseOrderApp.Domain.ValueObjects;
using PurchaseOrderApp.Infrastructure;

namespace PurchaseOrderApp.Tests.Shared;

public static class PurchaseOrderScenarioSeeder
{
    /// <summary>
    /// Seeds one approved purchase order with one reservable line and matching warehouse stock.
    /// </summary>
    public static async Task<ApprovedLineSeedResult> SeedApprovedLineAsync(
        DatabaseContext db,
        InventoryTrackingMode trackingMode = InventoryTrackingMode.Unit,
        decimal onHandQuantity = 100m,
        decimal quantityOrdered = 25m,
        decimal standardCost = 1.25m,
        CancellationToken cancellationToken = default)
    {
        var warehouse = Warehouse.Create("NSW", "New South Wales", TestData.User, TestData.OccurredAt);
        var item = CreateItem(trackingMode, standardCost);
        var stock = TestData.CreateWarehouseStock(warehouse.Id, item.Id, onHandQuantity);
        var purchaseOrder = CreateApprovedPurchaseOrder(warehouse.Id, item, quantityOrdered);
        var line = purchaseOrder.Lines.Single();

        await db.AddRangeAsync(warehouse, item, stock, purchaseOrder);
        await db.SaveChangesAsync(cancellationToken);
        db.ChangeTracker.Clear();

        return new ApprovedLineSeedResult(
            warehouse.Id,
            item.Id,
            purchaseOrder.Id,
            line.Id,
            stock.Id);
    }

    /// <summary>
    /// Seeds two approved purchase order lines for the same warehouse and item.
    /// This is used to test competing reservation requests against the same stock row.
    /// </summary>
    public static async Task<CompetingReservationSeedResult> SeedCompetingReservationScenarioAsync(
        DatabaseContext db,
        decimal onHandQuantity = 10m,
        decimal quantityOrdered = 7m,
        decimal standardCost = 1.25m,
        CancellationToken cancellationToken = default)
    {
        var warehouse = Warehouse.Create("NSW", "New South Wales", TestData.User, TestData.OccurredAt);
        var item = TestData.CreateUnitItem(standardCost: standardCost);
        var stock = TestData.CreateWarehouseStock(warehouse.Id, item.Id, onHandQuantity);

        var firstPurchaseOrder = CreateApprovedPurchaseOrder(warehouse.Id, item, quantityOrdered);
        var secondPurchaseOrder = CreateApprovedPurchaseOrder(warehouse.Id, item, quantityOrdered);

        await db.AddRangeAsync(warehouse, item, stock, firstPurchaseOrder, secondPurchaseOrder);
        await db.SaveChangesAsync(cancellationToken);
        db.ChangeTracker.Clear();

        return new CompetingReservationSeedResult(
            warehouse.Id,
            item.Id,
            firstPurchaseOrder.Id,
            secondPurchaseOrder.Id,
            firstPurchaseOrder.Lines.Single().Id,
            secondPurchaseOrder.Lines.Single().Id,
            stock.Id);
    }

    /// <summary>
    /// Seeds one warehouse with two stocked items so lock-boundary tests can prove different items do not block each other.
    /// </summary>
    public static async Task<TwoItemStockSeedResult> SeedTwoItemsInOneWarehouseAsync(
        DatabaseContext db,
        decimal firstOnHandQuantity = 10m,
        decimal secondOnHandQuantity = 10m,
        CancellationToken cancellationToken = default)
    {
        var warehouse = Warehouse.Create("NSW", "New South Wales", TestData.User, TestData.OccurredAt);
        var firstItem = TestData.CreateUnitItem("SHACKLE-12T", "12t Bow Shackle");
        var secondItem = TestData.CreateUnitItem("SLING-10T", "10t Chain Sling");
        var firstStock = TestData.CreateWarehouseStock(warehouse.Id, firstItem.Id, firstOnHandQuantity);
        var secondStock = TestData.CreateWarehouseStock(warehouse.Id, secondItem.Id, secondOnHandQuantity);

        await db.AddRangeAsync(warehouse, firstItem, secondItem, firstStock, secondStock);
        await db.SaveChangesAsync(cancellationToken);
        db.ChangeTracker.Clear();

        return new TwoItemStockSeedResult(
            warehouse.Id,
            firstItem.Id,
            secondItem.Id,
            firstStock.Id,
            secondStock.Id);
    }

    /// <summary>
    /// Seeds the same stocked item in two warehouses so lock-boundary tests can prove different warehouses do not block each other.
    /// </summary>
    public static async Task<TwoWarehouseStockSeedResult> SeedSameItemInTwoWarehousesAsync(
        DatabaseContext db,
        decimal firstOnHandQuantity = 10m,
        decimal secondOnHandQuantity = 10m,
        CancellationToken cancellationToken = default)
    {
        var firstWarehouse = Warehouse.Create("NSW", "New South Wales", TestData.User, TestData.OccurredAt);
        var secondWarehouse = Warehouse.Create("VIC", "Victoria", TestData.User, TestData.OccurredAt);
        var item = TestData.CreateUnitItem();
        var firstStock = TestData.CreateWarehouseStock(firstWarehouse.Id, item.Id, firstOnHandQuantity);
        var secondStock = TestData.CreateWarehouseStock(secondWarehouse.Id, item.Id, secondOnHandQuantity);

        await db.AddRangeAsync(firstWarehouse, secondWarehouse, item, firstStock, secondStock);
        await db.SaveChangesAsync(cancellationToken);
        db.ChangeTracker.Clear();

        return new TwoWarehouseStockSeedResult(
            firstWarehouse.Id,
            secondWarehouse.Id,
            item.Id,
            firstStock.Id,
            secondStock.Id);
    }

    public static async Task DeleteAsync(DatabaseContext db, CompetingReservationSeedResult seed, CancellationToken cancellationToken)
    {
        await db.AuditLogEntries.Where(entry => entry.WarehouseId == seed.WarehouseId).ExecuteDeleteAsync(cancellationToken);
        await db.StockReservations.Where(reservation => reservation.WarehouseId == seed.WarehouseId).ExecuteDeleteAsync(cancellationToken);
        await db.PurchaseOrderLines.Where(line => line.PurchaseOrderId == seed.FirstPurchaseOrderId || line.PurchaseOrderId == seed.SecondPurchaseOrderId).ExecuteDeleteAsync(cancellationToken);
        await db.PurchaseOrders.Where(order => order.Id == seed.FirstPurchaseOrderId || order.Id == seed.SecondPurchaseOrderId).ExecuteDeleteAsync(cancellationToken);
        await db.WarehouseStock.Where(stock => stock.Id == seed.WarehouseStockId).ExecuteDeleteAsync(cancellationToken);
        await db.InventoryItems.Where(item => item.Id == seed.InventoryItemId).ExecuteDeleteAsync(cancellationToken);
        await db.Warehouses.Where(warehouse => warehouse.Id == seed.WarehouseId).ExecuteDeleteAsync(cancellationToken);
    }

    private static InventoryItem CreateItem(InventoryTrackingMode trackingMode, decimal standardCost)
    {
        return trackingMode == InventoryTrackingMode.Weight
            ? TestData.CreateWeightItem(standardCost: standardCost)
            : TestData.CreateUnitItem(standardCost: standardCost);
    }

    private static PurchaseOrder CreateApprovedPurchaseOrder(WarehouseId warehouseId, InventoryItem item, decimal quantityOrdered)
    {
        var purchaseOrder = PurchaseOrder.CreatePending(warehouseId, TestData.User, TestData.OccurredAt);
        purchaseOrder.AddLine(item, new Quantity(quantityOrdered), TestData.User, TestData.OccurredAt);
        purchaseOrder.Approve(TestData.User, TestData.OccurredAt);
        return purchaseOrder;
    }

    public sealed record ApprovedLineSeedResult(
        WarehouseId WarehouseId,
        InventoryItemId InventoryItemId,
        PurchaseOrderId PurchaseOrderId,
        PurchaseOrderLineId PurchaseOrderLineId,
        WarehouseStockId WarehouseStockId);

    public sealed record CompetingReservationSeedResult(
        WarehouseId WarehouseId,
        InventoryItemId InventoryItemId,
        PurchaseOrderId FirstPurchaseOrderId,
        PurchaseOrderId SecondPurchaseOrderId,
        PurchaseOrderLineId FirstPurchaseOrderLineId,
        PurchaseOrderLineId SecondPurchaseOrderLineId,
        WarehouseStockId WarehouseStockId);

    public sealed record TwoItemStockSeedResult(
        WarehouseId WarehouseId,
        InventoryItemId FirstInventoryItemId,
        InventoryItemId SecondInventoryItemId,
        WarehouseStockId FirstWarehouseStockId,
        WarehouseStockId SecondWarehouseStockId);

    public sealed record TwoWarehouseStockSeedResult(
        WarehouseId FirstWarehouseId,
        WarehouseId SecondWarehouseId,
        InventoryItemId InventoryItemId,
        WarehouseStockId FirstWarehouseStockId,
        WarehouseStockId SecondWarehouseStockId);
}
