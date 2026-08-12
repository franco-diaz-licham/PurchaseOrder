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
        string purchaseOrderNumber,
        WarehouseId warehouseId,
        PurchaseOrderStatus status,
        string user,
        DateTimeOffset occurredAt)
    {
        Id = id;
        PurchaseOrderNumber = Required(purchaseOrderNumber, nameof(purchaseOrderNumber));
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

    public static PurchaseOrder CreatePending(
        string purchaseOrderNumber,
        WarehouseId warehouseId,
        string user,
        DateTimeOffset occurredAt)
    {
        return new PurchaseOrder(
            new PurchaseOrderId(Guid.NewGuid()),
            purchaseOrderNumber,
            warehouseId,
            PurchaseOrderStatus.Pending,
            user,
            occurredAt);
    }

    public PurchaseOrderLine AddLine(
        InventoryItem item,
        Quantity quantityOrdered,
        string user,
        DateTimeOffset occurredAt)
    {
        if (Status == PurchaseOrderStatus.Cancelled) throw new DomainException("Cancelled purchase orders cannot be changed.");
        if (Status == PurchaseOrderStatus.Closed) throw new DomainException("Closed purchase orders cannot be changed.");

        quantityOrdered.EnsureValidFor(item.TrackingMode);
        if (quantityOrdered.IsZero) throw new DomainException("Purchase order line quantity must be greater than zero.");

        var line = new PurchaseOrderLine(new PurchaseOrderLineId(Guid.NewGuid()), Id, item.Id, quantityOrdered, user, occurredAt);
        _lines.Add(line);
        SetUpdated(user, occurredAt);
        return line;
    }

    public void ReserveLine(PurchaseOrderLineId lineId, Quantity quantity, string user, DateTimeOffset occurredAt)
    {
        EnsureApproved();
        var line = GetLine(lineId);
        line.Reserve(quantity, user, occurredAt);
        SetUpdated(user, occurredAt);
    }

    public void ReleaseLine(PurchaseOrderLineId lineId, Quantity quantity, string user, DateTimeOffset occurredAt)
    {
        var line = GetLine(lineId);
        line.Release(quantity, user, occurredAt);
        SetUpdated(user, occurredAt);
    }

    public void Approve(string user, DateTimeOffset occurredAt)
    {
        if (Status == PurchaseOrderStatus.Cancelled) throw new DomainException("Cancelled purchase orders cannot be approved.");
        if (Status == PurchaseOrderStatus.Closed) throw new DomainException("Closed purchase orders cannot be approved.");

        Status = PurchaseOrderStatus.Approved;
        SetUpdated(user, occurredAt);
    }

    public void Close(string user, DateTimeOffset occurredAt)
    {
        Status = PurchaseOrderStatus.Closed;
        SetUpdated(user, occurredAt);
    }

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
