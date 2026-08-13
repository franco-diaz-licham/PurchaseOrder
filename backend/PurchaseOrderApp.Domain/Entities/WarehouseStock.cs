using PurchaseOrderApp.Domain.Core;
using PurchaseOrderApp.Domain.ValueObjects;

namespace PurchaseOrderApp.Domain.Entities;

/// <summary>
/// Aggregate root for the on-hand stock balance of a single inventory item at a single warehouse.
/// </summary>
public sealed class WarehouseStock : Entity<WarehouseStockId>
{
    private WarehouseStock() { }

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

    /// <summary>
    /// Ensures the requested reservation quantity does not exceed currently available stock.
    /// </summary>
    public void EnsureCanReserve(Quantity activeReservedQuantity, Quantity requestedQuantity)
    {
        var availableQuantity = OnHandQuantity.Subtract(activeReservedQuantity);
        if (requestedQuantity.Value > availableQuantity.Value) throw new DomainException("Reservation quantity exceeds available stock. Please refresh the page and try again.");
    }

    /// <summary>
    /// Calculates available stock by subtracting active reserved quantity from on-hand quantity.
    /// </summary>
    public Quantity CalculateAvailableQuantity(Quantity activeReservedQuantity) => OnHandQuantity.Subtract(activeReservedQuantity);
}
