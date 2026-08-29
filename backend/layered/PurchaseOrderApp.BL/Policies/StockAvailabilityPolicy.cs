using PurchaseOrderApp.BL.Common;
using PurchaseOrderApp.BL.Models;

namespace PurchaseOrderApp.BL.Policies;

public sealed class StockAvailabilityPolicy
{
    public Result CanReserve(WarehouseStock stock, decimal activeReservedQuantity, decimal requestedQuantity)
    {
        var availableQuantity = stock.OnHandQuantity - activeReservedQuantity;
        if (requestedQuantity > availableQuantity) {
            return Result.Fail("Reservation quantity exceeds available stock. Please refresh the page and try again.");
        }

        return Result.Success();
    }
}
