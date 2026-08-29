using PurchaseOrderApp.BL.Common;
using PurchaseOrderApp.BL.Enums;

namespace PurchaseOrderApp.BL.Policies;

public sealed class InventoryQuantityPolicy
{
    public Result CanUseQuantity(InventoryTrackingMode trackingMode, decimal quantity, string fieldName)
    {
        if (quantity <= 0) return Result.Fail($"{fieldName} must be greater than zero.");

        if (trackingMode == InventoryTrackingMode.Unit && quantity != decimal.Truncate(quantity)) {
            return Result.Fail("Unit-tracked items require whole-number quantities.");
        }

        if (trackingMode == InventoryTrackingMode.Weight && decimal.Round(quantity, 3) != quantity) {
            return Result.Fail("Weight-tracked items support up to 3 decimal places.");
        }

        return Result.Success();
    }

    public Result CanUseCost(decimal amount)
    {
        if (amount < 0) return Result.Fail("Money amount cannot be negative.");
        return Result.Success();
    }
}
