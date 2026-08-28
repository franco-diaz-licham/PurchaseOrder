namespace PurchaseOrderApp.Domain.Enums;

/// <summary>
/// Lifecycle state controlling whether a purchase order can receive reservations.
/// </summary>
public enum PurchaseOrderStatus
{
    /// <summary>
    /// The purchase order has been created but is not yet ready for stock reservation.
    /// </summary>
    Pending = 1,

    /// <summary>
    /// The purchase order has been approved and can receive stock reservations.
    /// </summary>
    Approved = 2,

    /// <summary>
    /// The purchase order is operationally complete and can no longer be changed.
    /// </summary>
    Closed = 3,

    /// <summary>
    /// The purchase order will not proceed and can no longer be changed.
    /// </summary>
    Cancelled = 4
}
