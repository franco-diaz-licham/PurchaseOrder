namespace PurchaseOrderApp.Domain.Enums;

/// <summary>
/// Type of stock action captured in the permanent audit log.
/// </summary>
public enum AuditAction
{
    /// <summary>
    /// Stock was reserved against a purchase order line.
    /// </summary>
    Reserve = 1,

    /// <summary>
    /// Stock was released from an active reservation.
    /// </summary>
    Release = 2
}
