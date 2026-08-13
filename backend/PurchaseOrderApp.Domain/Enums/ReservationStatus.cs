namespace PurchaseOrderApp.Domain.Enums;

/// <summary>
/// Lifecycle state of a stock reservation after creation and release.
/// </summary>
public enum ReservationStatus
{
    /// <summary>
    /// Reservation still contributes to committed stock and available quantity.
    /// </summary>
    Active = 1,

    /// <summary>
    /// Reservation has been fully released and no longer contributes to committed stock.
    /// </summary>
    Released = 2
}
