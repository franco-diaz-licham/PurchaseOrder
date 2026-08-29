using PurchaseOrderApp.BL.Common;
using PurchaseOrderApp.BL.Enums;

namespace PurchaseOrderApp.BL.Policies;

public sealed class InventoryQuantityPolicy
{
    public Result CanUseQuantity(InventoryTrackingMode trackingMode, decimal quantity, string fieldName)
    {
        if (quantity < 0) return Result.Fail("Quantity cannot be negative.");
        if (decimal.Round(quantity, 3) != quantity) return Result.Fail("Quantity cannot have more than 3 decimal places.");
        if (quantity == 0) return Result.Fail($"{fieldName} must be greater than zero.");
        if (trackingMode == InventoryTrackingMode.Unit && quantity != decimal.Truncate(quantity)) return Result.Fail("Unit-tracked item quantities must be whole numbers.");
        return Result.Success();
    }

    public Result CanUseCost(decimal amount)
    {
        if (amount < 0) return Result.Fail("Money cannot be negative.");
        return Result.Success();
    }
}
