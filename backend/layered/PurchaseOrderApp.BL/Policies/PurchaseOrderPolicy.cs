using PurchaseOrderApp.BL.Common;
using PurchaseOrderApp.BL.Enums;
using PurchaseOrderApp.BL.Models;

namespace PurchaseOrderApp.BL.Policies;

public sealed class PurchaseOrderPolicy
{
    public Result CanApprove(PurchaseOrder purchaseOrder)
    {
        if (purchaseOrder.Status == PurchaseOrderStatus.Cancelled) return Result.Fail("Cancelled purchase orders cannot be approved.");
        if (purchaseOrder.Status == PurchaseOrderStatus.Closed) return Result.Fail("Closed purchase orders cannot be approved.");

        return Result.Success();
    }

    public Result CanChangeLines(PurchaseOrder purchaseOrder)
    {
        if (purchaseOrder.Status == PurchaseOrderStatus.Cancelled) return Result.Fail("Cancelled purchase orders cannot be changed.");
        if (purchaseOrder.Status == PurchaseOrderStatus.Closed) return Result.Fail("Closed purchase orders cannot be changed.");

        return Result.Success();
    }

    public Result CanReserve(PurchaseOrder purchaseOrder)
    {
        if (purchaseOrder.Status != PurchaseOrderStatus.Approved) return Result.Fail("Only approved purchase orders can receive reservations.");

        return Result.Success();
    }

    public Result CanCancel(PurchaseOrder purchaseOrder)
    {
        if (purchaseOrder.Lines.Any(line => line.QuantityReserved > 0)) return Result.Fail("Purchase orders with active reservations cannot be cancelled.");

        return Result.Success();
    }
}
