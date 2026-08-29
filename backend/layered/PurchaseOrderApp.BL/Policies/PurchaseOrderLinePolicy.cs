using PurchaseOrderApp.BL.Common;
using PurchaseOrderApp.BL.Models;

namespace PurchaseOrderApp.BL.Policies;

public sealed class PurchaseOrderLinePolicy
{
    public Result CanReserve(PurchaseOrderLine line, decimal quantity)
    {
        if (quantity <= 0) return Result.Fail("Reservation quantity must be greater than zero.");
        if (quantity > line.QuantityRemaining) return Result.Fail("Reservation quantity exceeds the purchase order line remaining quantity.");

        return Result.Success();
    }

    public Result CanRelease(PurchaseOrderLine line, decimal quantity)
    {
        if (quantity <= 0) return Result.Fail("Release quantity must be greater than zero.");
        if (quantity > line.QuantityReserved) return Result.Fail("Release quantity exceeds the purchase order line reserved quantity.");

        return Result.Success();
    }

    public Result CanUpdateQuantity(PurchaseOrderLine line, decimal quantityOrdered)
    {
        if (quantityOrdered <= 0) return Result.Fail("Purchase order line quantity must be greater than zero.");
        if (quantityOrdered < line.QuantityReserved) return Result.Fail("Purchase order line quantity cannot be less than the reserved quantity.");

        return Result.Success();
    }
}
