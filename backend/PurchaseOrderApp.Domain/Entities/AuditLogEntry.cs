using PurchaseOrderApp.Domain.Core;
using PurchaseOrderApp.Domain.Enums;
using PurchaseOrderApp.Domain.ValueObjects;

namespace PurchaseOrderApp.Domain.Entities;

/// <summary>
/// Immutable record of a successful stock reservation or release action.
/// </summary>
public sealed class AuditLogEntry : Entity<AuditLogEntryId>
{
    private AuditLogEntry() { }
    private AuditLogEntry(
        AuditLogEntryId id,
        AuditAction action,
        InventoryItemId inventoryItemId,
        WarehouseId warehouseId,
        PurchaseOrderLineId purchaseOrderLineId,
        StockReservationId stockReservationId,
        Quantity quantity,
        Quantity resultingAvailableQuantity,
        string user,
        DateTimeOffset occurredAt)
    {
        if (quantity.IsZero) throw new DomainException("Audit quantity must be greater than zero.");

        Id = id;
        Action = action;
        InventoryItemId = inventoryItemId;
        WarehouseId = warehouseId;
        PurchaseOrderLineId = purchaseOrderLineId;
        StockReservationId = stockReservationId;
        Quantity = quantity;
        ResultingAvailableQuantity = resultingAvailableQuantity;
        SetCreated(user, occurredAt);
    }

    /// <summary>
    /// Reservation or release action recorded by this audit entry.
    /// </summary>
    public AuditAction Action { get; private set; }

    /// <summary>
    /// Item affected by the audited stock action.
    /// </summary>
    public InventoryItemId InventoryItemId { get; private set; }

    /// <summary>
    /// Warehouse where the audited stock action occurred.
    /// </summary>
    public WarehouseId WarehouseId { get; private set; }

    /// <summary>
    /// Purchase order line affected by the audited stock action.
    /// </summary>
    public PurchaseOrderLineId PurchaseOrderLineId { get; private set; }

    /// <summary>
    /// Stock reservation affected by the audited stock action.
    /// </summary>
    public StockReservationId StockReservationId { get; private set; }

    /// <summary>
    /// Quantity reserved or released by the audited action.
    /// </summary>
    public Quantity Quantity { get; private set; }

    /// <summary>
    /// Available stock balance after the audited action completed.
    /// </summary>
    public Quantity ResultingAvailableQuantity { get; private set; }

    public static AuditLogEntry RecordReservation(
        StockReservation reservation,
        Quantity resultingAvailableQuantity,
        string user,
        DateTimeOffset occurredAt)
    {
        return new AuditLogEntry(
            new AuditLogEntryId(Guid.NewGuid()),
            AuditAction.Reserve,
            reservation.InventoryItemId,
            reservation.WarehouseId,
            reservation.PurchaseOrderLineId,
            reservation.Id,
            reservation.QuantityReserved,
            resultingAvailableQuantity,
            user,
            occurredAt);
    }

    public static AuditLogEntry RecordRelease(
        StockReservation reservation,
        Quantity releasedQuantity,
        Quantity resultingAvailableQuantity,
        string user,
        DateTimeOffset occurredAt)
    {
        return new AuditLogEntry(
            new AuditLogEntryId(Guid.NewGuid()),
            AuditAction.Release,
            reservation.InventoryItemId,
            reservation.WarehouseId,
            reservation.PurchaseOrderLineId,
            reservation.Id,
            releasedQuantity,
            resultingAvailableQuantity,
            user,
            occurredAt);
    }
}
