namespace PurchaseOrderApp.Domain.Enums;

/// <summary>
/// Quantity tracking precision used by an inventory item.
/// </summary>
public enum InventoryTrackingMode
{
    /// <summary>
    /// Quantity must be a whole number.
    /// </summary>
    Unit = 1,

    /// <summary>
    /// Quantity can use up to 3 decimal places.
    /// </summary>
    Weight = 2
}
