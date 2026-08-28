using PurchaseOrderApp.Domain.Core;
using PurchaseOrderApp.Domain.Enums;
using PurchaseOrderApp.Domain.Events;
using PurchaseOrderApp.Domain.ValueObjects;

namespace PurchaseOrderApp.Domain.Entities;

/// <summary>
/// Aggregate root for an active or released stock commitment against a purchase order line.
/// </summary>
public sealed class StockReservation : Entity<StockReservationId>
{
    private StockReservation() { }

    private StockReservation(
        StockReservationId id,
        PurchaseOrderLineId purchaseOrderLineId,
        WarehouseId warehouseId,
        InventoryItemId inventoryItemId,
        Quantity quantityReserved,
        Money unitCostSnapshot,
        string user,
        DateTimeOffset occurredAt)
    {
        if (quantityReserved.IsZero) throw new DomainException("Reservation quantity must be greater than zero.");

        Id = id;
        PurchaseOrderLineId = purchaseOrderLineId;
        WarehouseId = warehouseId;
        InventoryItemId = inventoryItemId;
        QuantityReserved = quantityReserved;
        UnitCostSnapshot = unitCostSnapshot;
        Status = ReservationStatus.Active;
        SetCreated(user, occurredAt);
    }

    /// <summary>
    /// Purchase order line this reservation commits stock against.
    /// </summary>
    public PurchaseOrderLineId PurchaseOrderLineId { get; private set; }

    /// <summary>
    /// Warehouse where the reserved stock is held.
    /// </summary>
    public WarehouseId WarehouseId { get; private set; }

    /// <summary>
    /// Inventory item reserved from warehouse stock.
    /// </summary>
    public InventoryItemId InventoryItemId { get; private set; }

    /// <summary>
    /// Currently active quantity reserved by this reservation.
    /// </summary>
    public Quantity QuantityReserved { get; private set; }

    /// <summary>
    /// Standard cost captured when the reservation was created.
    /// </summary>
    public Money UnitCostSnapshot { get; private set; }

    /// <summary>
    /// Current lifecycle state of the reservation.
    /// </summary>
    public ReservationStatus Status { get; private set; }

    /// <summary>
    /// Current committed value based on active quantity and the cost snapshot.
    /// </summary>
    public Money CommittedValue => new(UnitCostSnapshot.Value * QuantityReserved.Value);

    /// <summary>
    /// Creates a new active stock reservation and raises the reserved event.
    /// </summary>
    internal static StockReservation Create(
        PurchaseOrderLineId purchaseOrderLineId,
        WarehouseId warehouseId,
        InventoryItem item,
        Quantity quantityReserved,
        Quantity resultingAvailableQuantity,
        string user,
        DateTimeOffset occurredAt)
    {
        quantityReserved.EnsureValidFor(item.TrackingMode);

        var reservation = new StockReservation(
            new StockReservationId(Guid.NewGuid()),
            purchaseOrderLineId,
            warehouseId,
            item.Id,
            quantityReserved,
            item.StandardCost,
            user,
            occurredAt);

        reservation.RaiseDomainEvent(new StockReservedEvent(
            reservation.Id,
            reservation.PurchaseOrderLineId,
            reservation.WarehouseId,
            reservation.InventoryItemId,
            reservation.QuantityReserved,
            resultingAvailableQuantity,
            user,
            occurredAt));

        return reservation;
    }

    /// <summary>
    /// Releases part or all of the reservation and raises the released event.
    /// </summary>
    internal void Release(Quantity quantity, Quantity resultingAvailableQuantity, string user, DateTimeOffset occurredAt)
    {
        if (Status != ReservationStatus.Active) throw new DomainException("Only active reservations can be released.");
        if (quantity.IsZero) throw new DomainException("Release quantity must be greater than zero.");
        if (quantity.Value > QuantityReserved.Value) throw new DomainException("Release quantity exceeds the active reservation quantity.");

        QuantityReserved = QuantityReserved.Subtract(quantity);
        if (QuantityReserved.IsZero) Status = ReservationStatus.Released;

        SetUpdated(user, occurredAt);
        RaiseDomainEvent(new StockReleasedEvent(
            Id,
            PurchaseOrderLineId,
            WarehouseId,
            InventoryItemId,
            quantity,
            resultingAvailableQuantity,
            user,
            occurredAt));
    }
}
