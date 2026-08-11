using PurchaseOrderApp.Domain.Core;
using PurchaseOrderApp.Domain.ValueObjects;

namespace PurchaseOrderApp.Domain.Entities;

/// <summary>
/// Aggregate root for the on-hand stock balance of a single inventory item at a single warehouse.
/// </summary>
public sealed class WarehouseStock : Entity<WarehouseStockId>
{
    private WarehouseStock() { }

    private WarehouseStock(
        WarehouseStockId id,
        WarehouseId warehouseId,
        InventoryItemId inventoryItemId,
        Quantity onHandQuantity,
        string user,
        DateTimeOffset occurredAt)
    {
        Id = id;
        WarehouseId = warehouseId;
        InventoryItemId = inventoryItemId;
        OnHandQuantity = onHandQuantity;
        SetCreated(user, occurredAt);
    }

    /// <summary>
    /// Warehouse that holds this stock balance.
    /// </summary>
    public WarehouseId WarehouseId { get; private set; }

    /// <summary>
    /// Inventory item held in this warehouse stock balance.
    /// </summary>
    public InventoryItemId InventoryItemId { get; private set; }

    /// <summary>
    /// Physical on-hand quantity before active reservations are subtracted.
    /// </summary>
    public Quantity OnHandQuantity { get; private set; }

    public static WarehouseStock Create(
        WarehouseId warehouseId,
        InventoryItem item,
        Quantity onHandQuantity,
        string user,
        DateTimeOffset occurredAt)
    {
        onHandQuantity.EnsureValidFor(item.TrackingMode);

        return new WarehouseStock(
            new WarehouseStockId(Guid.NewGuid()),
            warehouseId,
            item.Id,
            onHandQuantity,
            user,
            occurredAt);
    }

    public void AdjustOnHandQuantity(InventoryItem item, Quantity onHandQuantity, string user, DateTimeOffset occurredAt)
    {
        if (item.Id != InventoryItemId) throw new DomainException("Inventory item does not match this warehouse stock.");

        onHandQuantity.EnsureValidFor(item.TrackingMode);
        OnHandQuantity = onHandQuantity;
        SetUpdated(user, occurredAt);
    }

    public void EnsureCanReserve(Quantity activeReservedQuantity, Quantity requestedQuantity)
    {
        var availableQuantity = OnHandQuantity.Subtract(activeReservedQuantity);
        if (requestedQuantity.Value > availableQuantity.Value) throw new DomainException("Reservation quantity exceeds available stock.");
    }

    public Quantity CalculateAvailableQuantity(Quantity activeReservedQuantity) => OnHandQuantity.Subtract(activeReservedQuantity);
}
