using PurchaseOrderApp.Domain.Core;
using PurchaseOrderApp.Domain.Enums;
using PurchaseOrderApp.Domain.ValueObjects;

namespace PurchaseOrderApp.Domain.Entities;

/// <summary>
/// Aggregate root for a warehouse purchase order and its reservable lines.
/// </summary>
public sealed class PurchaseOrder : Entity<PurchaseOrderId>
{
    private readonly List<PurchaseOrderLine> _lines = [];

    private PurchaseOrder() { }

    private PurchaseOrder(
        PurchaseOrderId id,
        WarehouseId warehouseId,
        PurchaseOrderStatus status,
        string user,
        DateTimeOffset occurredAt)
    {
        Id = id;
        WarehouseId = warehouseId;
        Status = status;
        SetCreated(user, occurredAt);
    }

    /// <summary>
    /// Human-readable purchase order number.
    /// </summary>
    public string PurchaseOrderNumber { get; private set; } = default!;

    /// <summary>
    /// Warehouse responsible for fulfilling the purchase order.
    /// </summary>
    public WarehouseId WarehouseId { get; private set; }

    /// <summary>
    /// Warehouse responsible for fulfilling the purchase order.
    /// </summary>
    public Warehouse Warehouse { get; private set; } = null!;

    /// <summary>
    /// Current lifecycle state of the purchase order.
    /// </summary>
    public PurchaseOrderStatus Status { get; private set; }

    /// <summary>
    /// Line items that can receive stock reservations.
    /// </summary>
    public IReadOnlyCollection<PurchaseOrderLine> Lines => _lines.AsReadOnly();

    /// <summary>
    /// Indicates whether any line still has quantity left to reserve.
    /// </summary>
    public bool HasOutstandingLines => _lines.Any(line => line.HasOutstandingQuantity);

    /// <summary>
    /// Creates a new purchase order in the pending state.
    /// </summary>
    public static PurchaseOrder CreatePending(WarehouseId warehouseId, string user, DateTimeOffset occurredAt)
    {
        return new PurchaseOrder(
            new PurchaseOrderId(Guid.NewGuid()),
            warehouseId,
            PurchaseOrderStatus.Pending,
            user,
            occurredAt);
    }

    /// <summary>
    /// Adds a new inventory item line to a purchase order that is still open for changes.
    /// </summary>
    public PurchaseOrderLine AddLine(InventoryItem item, Quantity quantityOrdered, string user, DateTimeOffset occurredAt)
    {
        if (Status == PurchaseOrderStatus.Cancelled) throw new DomainException("Cancelled purchase orders cannot be changed.");
        if (Status == PurchaseOrderStatus.Closed) throw new DomainException("Closed purchase orders cannot be changed.");
        if (_lines.Any(line => line.InventoryItemId == item.Id)) throw new DomainException("Inventory item has already been added to this purchase order.");

        quantityOrdered.EnsureValidFor(item.TrackingMode);
        if (quantityOrdered.IsZero) throw new DomainException("Purchase order line quantity must be greater than zero.");

        var line = new PurchaseOrderLine(new PurchaseOrderLineId(Guid.NewGuid()), Id, item, quantityOrdered, user, occurredAt);
        _lines.Add(line);
        SetUpdated(user, occurredAt);
        return line;
    }

    /// <summary>
    /// Records reserved quantity against an approved purchase order line.
    /// </summary>
    public void ReserveLine(PurchaseOrderLineId lineId, Quantity quantity, string user, DateTimeOffset occurredAt)
    {
        EnsureApproved();
        var line = GetLine(lineId);
        line.Reserve(quantity, user, occurredAt);
        SetUpdated(user, occurredAt);
    }

    /// <summary>
    /// Releases reserved quantity from a purchase order line.
    /// </summary>
    public void ReleaseLine(PurchaseOrderLineId lineId, Quantity quantity, string user, DateTimeOffset occurredAt)
    {
        var line = GetLine(lineId);
        line.Release(quantity, user, occurredAt);
        SetUpdated(user, occurredAt);
    }

    /// <summary>
    /// Removes a purchase order line after its reservations have been released.
    /// </summary>
    public void RemoveLine(PurchaseOrderLineId lineId, string user, DateTimeOffset occurredAt)
    {
        if (Status == PurchaseOrderStatus.Cancelled) throw new DomainException("Cancelled purchase orders cannot be changed.");
        if (Status == PurchaseOrderStatus.Closed) throw new DomainException("Closed purchase orders cannot be changed.");

        var line = GetLine(lineId);
        if (!line.QuantityReserved.IsZero) throw new DomainException("Purchase order line reservations must be released before the line can be removed.");

        _lines.Remove(line);
        SetUpdated(user, occurredAt);
    }

    /// <summary>
    /// Updates ordered quantity while preserving the invariant that ordered quantity cannot be less than reserved quantity.
    /// </summary>
    public void UpdateLineQuantity(PurchaseOrderLineId lineId, Quantity quantityOrdered, string user, DateTimeOffset occurredAt)
    {
        if (Status == PurchaseOrderStatus.Cancelled) throw new DomainException("Cancelled purchase orders cannot be changed.");
        if (Status == PurchaseOrderStatus.Closed) throw new DomainException("Closed purchase orders cannot be changed.");

        var line = GetLine(lineId);
        line.UpdateQuantity(quantityOrdered, user, occurredAt);
        SetUpdated(user, occurredAt);
    }

    /// <summary>
    /// Approves the purchase order so stock can be reserved against its lines.
    /// </summary>
    public void Approve(string user, DateTimeOffset occurredAt)
    {
        if (Status == PurchaseOrderStatus.Cancelled) throw new DomainException("Cancelled purchase orders cannot be approved.");
        if (Status == PurchaseOrderStatus.Closed) throw new DomainException("Closed purchase orders cannot be approved.");

        Status = PurchaseOrderStatus.Approved;
        SetUpdated(user, occurredAt);
    }

    /// <summary>
    /// Closes the purchase order as operationally complete. Closed purchase orders are read-only.
    /// </summary>
    public void Close(string user, DateTimeOffset occurredAt)
    {
        Status = PurchaseOrderStatus.Closed;
        SetUpdated(user, occurredAt);
    }

    /// <summary>
    /// Cancels a purchase order that has no reserved stock.
    /// </summary>
    public void Cancel(string user, DateTimeOffset occurredAt)
    {
        if (_lines.Any(line => !line.QuantityReserved.IsZero)) throw new DomainException("Purchase orders with active reservations cannot be cancelled.");

        Status = PurchaseOrderStatus.Cancelled;
        SetUpdated(user, occurredAt);
    }

    private PurchaseOrderLine GetLine(PurchaseOrderLineId lineId)
    {
        var line = _lines.SingleOrDefault(item => item.Id == lineId);
        if (line is null) throw new DomainException("Purchase order line was not found.");
        return line;
    }

    private void EnsureApproved()
    {
        if (Status != PurchaseOrderStatus.Approved) throw new DomainException("Only approved purchase orders can receive reservations.");
    }
}
