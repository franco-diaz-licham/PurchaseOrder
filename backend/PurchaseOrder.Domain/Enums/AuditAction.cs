namespace PurchaseOrder.Domain.Enums;

/// <summary>
/// Type of stock action captured in the permanent audit log.
/// </summary>
public enum AuditAction
{
    Reserve = 1,
    Release = 2
}
