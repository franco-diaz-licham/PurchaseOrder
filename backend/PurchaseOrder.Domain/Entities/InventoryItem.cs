using PurchaseOrder.Domain.Core;
using PurchaseOrder.Domain.Enums;
using PurchaseOrder.Domain.ValueObjects;

namespace PurchaseOrder.Domain.Entities;

/// <summary>
/// Aggregate root for a stocked item with tracking precision, category, and current standard cost.
/// </summary>
public sealed class InventoryItem : Entity<InventoryItemId>
{
    private InventoryItem() { }

    private InventoryItem(
        InventoryItemId id,
        string sku,
        string name,
        InventoryItemCategory category,
        InventoryTrackingMode trackingMode,
        Money standardCost,
        string user,
        DateTimeOffset occurredAt)
    {
        Id = id;
        Sku = Required(sku, nameof(sku));
        Name = Required(name, nameof(name));
        Category = category;
        TrackingMode = trackingMode;
        StandardCost = standardCost;
        SetCreated(user, occurredAt);
    }

    /// <summary>
    /// Business stock-keeping unit used to identify the item.
    /// </summary>
    public string Sku { get; private set; } = default!;

    /// <summary>
    /// Display name for the stocked item.
    /// </summary>
    public string Name { get; private set; } = default!;

    /// <summary>
    /// Business category assigned to the stocked item.
    /// </summary>
    public InventoryItemCategory Category { get; private set; }

    /// <summary>
    /// Quantity tracking mode that controls valid reservation precision.
    /// </summary>
    public InventoryTrackingMode TrackingMode { get; private set; }

    /// <summary>
    /// Current standard cost used as the snapshot value for new reservations.
    /// </summary>
    public Money StandardCost { get; private set; }

    public static InventoryItem Create(
        string sku,
        string name,
        InventoryItemCategory category,
        InventoryTrackingMode trackingMode,
        Money standardCost,
        string user,
        DateTimeOffset occurredAt)
    {
        return new InventoryItem(
            new InventoryItemId(Guid.NewGuid()),
            sku,
            name,
            category,
            trackingMode,
            standardCost,
            user,
            occurredAt);
    }

    public void EnsureValidQuantity(Quantity quantity) => quantity.EnsureValidFor(TrackingMode);

    public void ChangeStandardCost(Money standardCost, string user, DateTimeOffset occurredAt)
    {
        StandardCost = standardCost;
        SetUpdated(user, occurredAt);
    }
}
