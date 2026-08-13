using PurchaseOrderApp.Domain.Core;
using PurchaseOrderApp.Domain.ValueObjects;

namespace PurchaseOrderApp.Domain.Entities;

/// <summary>
/// Purchase order line item that tracks ordered, reserved, and remaining quantity.
/// </summary>
public sealed class PurchaseOrderLine : Entity<PurchaseOrderLineId>
{
    private PurchaseOrderLine() { }

    /// <summary>
    /// Creates a purchase order line owned by the purchase order aggregate.
    /// </summary>
    internal PurchaseOrderLine(
        PurchaseOrderLineId id,
        PurchaseOrderId purchaseOrderId,
        InventoryItem inventoryItem,
        Quantity quantityOrdered,
        string user,
        DateTimeOffset occurredAt)
    {
        Id = id;
        PurchaseOrderId = purchaseOrderId;
        InventoryItemId = inventoryItem.Id;
        InventoryItem = inventoryItem;
        QuantityOrdered = quantityOrdered;
        QuantityReserved = Quantity.Zero;
        SetCreated(user, occurredAt);
    }

    /// <summary>
    /// Purchase order that owns this line.
    /// </summary>
    public PurchaseOrderId PurchaseOrderId { get; private set; }

    /// <summary>
    /// Inventory item requested by this line.
    /// </summary>
    public InventoryItemId InventoryItemId { get; private set; }

    /// <summary>
    /// Inventory item requested by this line.
    /// </summary>
    public InventoryItem InventoryItem { get; private set; } = null!;

    /// <summary>
    /// Total quantity ordered for the item.
    /// </summary>
    public Quantity QuantityOrdered { get; private set; }

    /// <summary>
    /// Quantity already reserved against this line.
    /// </summary>
    public Quantity QuantityReserved { get; private set; }

    /// <summary>
    /// Quantity still available to reserve on this line.
    /// </summary>
    public Quantity QuantityRemaining => QuantityOrdered.Subtract(QuantityReserved);

    /// <summary>
    /// Indicates whether this line still has quantity left to reserve.
    /// </summary>
    public bool HasOutstandingQuantity => QuantityReserved.Value < QuantityOrdered.Value;

    /// <summary>
    /// Indicates whether the ordered quantity has been fully reserved.
    /// </summary>
    public bool IsFullyReserved => QuantityReserved == QuantityOrdered;

    /// <summary>
    /// Increases reserved quantity while ensuring the line is not over-reserved.
    /// </summary>
    internal void Reserve(Quantity quantity, string user, DateTimeOffset occurredAt)
    {
        if (quantity.IsZero) throw new DomainException("Reservation quantity must be greater than zero.");
        if (quantity.Value > QuantityRemaining.Value) throw new DomainException("Reservation quantity exceeds the purchase order line remaining quantity.");

        QuantityReserved = QuantityReserved.Add(quantity);
        SetUpdated(user, occurredAt);
    }

    /// <summary>
    /// Decreases reserved quantity when stock is released.
    /// </summary>
    internal void Release(Quantity quantity, string user, DateTimeOffset occurredAt)
    {
        if (quantity.IsZero) throw new DomainException("Release quantity must be greater than zero.");
        if (quantity.Value > QuantityReserved.Value) throw new DomainException("Release quantity exceeds the purchase order line reserved quantity.");

        QuantityReserved = QuantityReserved.Subtract(quantity);
        SetUpdated(user, occurredAt);
    }

    /// <summary>
    /// Changes ordered quantity while keeping existing reservations valid.
    /// </summary>
    internal void UpdateQuantity(Quantity quantityOrdered, string user, DateTimeOffset occurredAt)
    {
        if (quantityOrdered.IsZero) throw new DomainException("Purchase order line quantity must be greater than zero.");
        if (quantityOrdered.Value < QuantityReserved.Value) throw new DomainException("Purchase order line quantity cannot be less than the reserved quantity.");

        quantityOrdered.EnsureValidFor(InventoryItem.TrackingMode);
        QuantityOrdered = quantityOrdered;
        SetUpdated(user, occurredAt);
    }
}
