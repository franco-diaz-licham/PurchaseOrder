namespace PurchaseOrderApp.Domain.Enums;

/// <summary>
/// Business category used to classify stocked inventory items.
/// </summary>
public enum InventoryItemCategory
{
    /// <summary>
    /// General stocked item.
    /// </summary>
    General = 1,

    /// <summary>
    /// Bulk item usually tracked by weight.
    /// </summary>
    BulkGoods = 2,

    /// <summary>
    /// Perishable stocked item.
    /// </summary>
    Perishable = 3,

    /// <summary>
    /// Hardware or crane-related component.
    /// </summary>
    Hardware = 4
}
