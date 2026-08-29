using PurchaseOrderApp.BL.Common;
using PurchaseOrderApp.BL.Enums;
using PurchaseOrderApp.BL.Models;

namespace PurchaseOrderApp.BL.Policies;

public sealed class ReservationPolicy
{
    public Result CanRelease(StockReservation reservation, decimal quantity)
    {
        if (reservation.Status != ReservationStatus.Active) return Result.Fail("Only active reservations can be released.");
        if (quantity <= 0) return Result.Fail("Release quantity must be greater than zero.");
        if (quantity > reservation.QuantityReserved) return Result.Fail("Release quantity exceeds the active reservation quantity.");

        return Result.Success();
    }
}
