using PurchaseOrderApp.BL.Enums;

namespace PurchaseOrderApp.BL.Models;

public sealed class InventoryItem : EntityMetadata
{
    public Guid Id { get; set; }

    public string Sku { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public InventoryItemCategory Category { get; set; }

    public InventoryTrackingMode TrackingMode { get; set; }

    public decimal StandardCost { get; set; }
}
