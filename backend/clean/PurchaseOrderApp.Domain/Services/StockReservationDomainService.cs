using PurchaseOrderApp.Domain.Entities;
using PurchaseOrderApp.Domain.ValueObjects;

namespace PurchaseOrderApp.Domain.Services;

/// <summary>
/// Coordinates reservation behavior that spans purchase orders, reservations, and warehouse stock.
/// </summary>
public static class StockReservationDomainService
{
    /// <summary>
    /// Reserves stock for a purchase order line after validating available warehouse stock.
    /// </summary>
    public static StockReservation Reserve(
        PurchaseOrder purchaseOrder,
        PurchaseOrderLineId purchaseOrderLineId,
        WarehouseStock stock,
        InventoryItem item,
        Quantity activeReservedQuantity,
        Quantity quantity,
        string user,
        DateTimeOffset occurredAt)
    {
        stock.EnsureCanReserve(activeReservedQuantity, quantity);
        purchaseOrder.ReserveLine(purchaseOrderLineId, quantity, user, occurredAt);

        var resultingAvailableQuantity = stock.CalculateAvailableQuantity(activeReservedQuantity.Add(quantity));
        return StockReservation.Create(
            purchaseOrderLineId,
            stock.WarehouseId,
            item,
            quantity,
            resultingAvailableQuantity,
            user,
            occurredAt);
    }

    /// <summary>
    /// Releases stock from an active reservation and updates the owning purchase order line.
    /// </summary>
    public static void Release(
        PurchaseOrder purchaseOrder,
        StockReservation reservation,
        WarehouseStock stock,
        Quantity activeReservedQuantity,
        Quantity quantity,
        string user,
        DateTimeOffset occurredAt)
    {
        var resultingAvailableQuantity = stock.CalculateAvailableQuantity(activeReservedQuantity.Subtract(quantity));
        reservation.Release(quantity, resultingAvailableQuantity, user, occurredAt);
        purchaseOrder.ReleaseLine(reservation.PurchaseOrderLineId, quantity, user, occurredAt);
    }
}
