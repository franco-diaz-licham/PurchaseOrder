namespace PurchaseOrder.Domain.Enums;

/// <summary>
/// Lifecycle state of a stock reservation after creation and release.
/// </summary>
public enum ReservationStatus
{
    Active = 1,
    Released = 2
}
