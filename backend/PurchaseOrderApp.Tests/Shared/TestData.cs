using PurchaseOrderApp.Domain.Entities;
using PurchaseOrderApp.Domain.Core;
using PurchaseOrderApp.Domain.Enums;
using PurchaseOrderApp.Domain.ValueObjects;
using System.Reflection;

namespace PurchaseOrderApp.Tests.Shared;

/// <summary>
/// Provides shared domain objects and values used by unit and integration tests.
/// </summary>
internal static class TestData
{
    /// <summary>
    /// Default actor used when tests need a user name.
    /// </summary>
    public const string User = "Franco Diaz";

    /// <summary>
    /// Default timestamp used to keep test data deterministic.
    /// </summary>
    public static readonly DateTimeOffset OccurredAt = new(2026, 8, 12, 10, 30, 0, TimeSpan.Zero);

    /// <summary>
    /// Default warehouse id used by simple domain tests.
    /// </summary>
    public static WarehouseId WarehouseId => new(Guid.Parse("11111111-1111-1111-1111-111111111111"));

    /// <summary>
    /// Creates a pending purchase order with the default warehouse, user, and timestamp.
    /// </summary>
    public static PurchaseOrder CreatePendingPurchaseOrder()
    {
        return PurchaseOrder.CreatePending(WarehouseId, User, OccurredAt);
    }

    /// <summary>
    /// Creates a unit-tracked inventory item for tests.
    /// </summary>
    public static InventoryItem CreateUnitItem(string sku = "BOLT-10", string name = "10mm Bolt", decimal standardCost = 1.25m)
    {
        return InventoryItem.Create(
            sku,
            name,
            InventoryItemCategory.Hardware,
            InventoryTrackingMode.Unit,
            new Money(standardCost),
            User,
            OccurredAt);
    }

    /// <summary>
    /// Creates a weight-tracked inventory item for tests.
    /// </summary>
    public static InventoryItem CreateWeightItem(string sku = "RICE-BULK", string name = "Bulk Rice", decimal standardCost = 1.75m)
    {
        return InventoryItem.Create(
            sku,
            name,
            InventoryItemCategory.BulkGoods,
            InventoryTrackingMode.Weight,
            new Money(standardCost),
            User,
            OccurredAt);
    }

    /// <summary>
    /// Creates warehouse stock for tests that need seeded stock without an application create flow.
    /// </summary>
    public static WarehouseStock CreateWarehouseStock(WarehouseId? warehouseId = null, InventoryItemId? inventoryItemId = null, decimal onHandQuantity = 100)
    {
        var stock = (WarehouseStock)Activator.CreateInstance(typeof(WarehouseStock), nonPublic: true)!;
        SetProperty(stock, nameof(WarehouseStock.Id), new WarehouseStockId(Guid.NewGuid()));
        SetProperty(stock, nameof(WarehouseStock.WarehouseId), warehouseId ?? WarehouseId);
        SetProperty(stock, nameof(WarehouseStock.InventoryItemId), inventoryItemId ?? new InventoryItemId(Guid.NewGuid()));
        SetProperty(stock, nameof(WarehouseStock.OnHandQuantity), new Quantity(onHandQuantity));
        SetCreated(stock);
        return stock;
    }

    /// <summary>
    /// Sets a property on an EF-friendly entity that does not expose a public setter.
    /// </summary>
    private static void SetProperty<T>(object instance, string propertyName, T value)
    {
        var property = instance.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        property!.SetValue(instance, value);
    }

    /// <summary>
    /// Applies the shared created audit values to test entities.
    /// </summary>
    private static void SetCreated(Entity entity)
    {
        var method = typeof(Entity).GetMethod("SetCreated", BindingFlags.Instance | BindingFlags.NonPublic);
        method!.Invoke(entity, [User, OccurredAt]);
    }
}
