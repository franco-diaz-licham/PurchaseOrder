using PurchaseOrderApp.Application.Models;
using PurchaseOrderApp.Domain.Entities;

namespace PurchaseOrderApp.Application.Helpers;

public static class PurchaseOrderMapper
{
    public static PurchaseOrderResponse ToResponse(PurchaseOrder purchaseOrder)
    {
        return new PurchaseOrderResponse(
            purchaseOrder.Id.Value,
            purchaseOrder.PurchaseOrderNumber,
            purchaseOrder.WarehouseId.Value,
            purchaseOrder.Status.ToString(),
            purchaseOrder.Lines
                .Select(line => new PurchaseOrderLineResponse(
                    line.Id.Value,
                    line.InventoryItemId.Value,
                    line.QuantityOrdered.Value,
                    line.QuantityReserved.Value,
                    line.QuantityRemaining.Value))
                .ToList());
    }
}
