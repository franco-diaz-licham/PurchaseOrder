namespace PurchaseOrderApp.Domain.Enums;

/// <summary>
/// Lifecycle state controlling whether a purchase order can receive reservations.
/// </summary>
public enum PurchaseOrderStatus
{
    Draft = 1,
    Approved = 2,
    Closed = 3,
    Cancelled = 4
}
