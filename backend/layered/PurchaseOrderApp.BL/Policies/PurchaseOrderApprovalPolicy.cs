using PurchaseOrderApp.BL.Common;
using PurchaseOrderApp.BL.Enums;
using PurchaseOrderApp.BL.Models;

namespace PurchaseOrderApp.BL.Policies;

public sealed class PurchaseOrderApprovalPolicy
{
    public Result CanApprove(PurchaseOrder purchaseOrder)
    {
        if (purchaseOrder.Status == PurchaseOrderStatus.Cancelled) {
            return Result.Fail("Cancelled purchase orders cannot be approved.");
        }

        if (purchaseOrder.Status == PurchaseOrderStatus.Closed) {
            return Result.Fail("Closed purchase orders cannot be approved.");
        }

        return Result.Success();
    }
}
