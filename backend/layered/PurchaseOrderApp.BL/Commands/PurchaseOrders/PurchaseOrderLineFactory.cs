using PurchaseOrderApp.BL.Models;

namespace PurchaseOrderApp.BL.Commands.PurchaseOrders;

internal static class PurchaseOrderLineFactory
{
    public static PurchaseOrderLine Create(
        Guid purchaseOrderId,
        InventoryItem item,
        decimal quantityOrdered,
        string user,
        DateTimeOffset occurredAt)
    {
        return new PurchaseOrderLine {
            Id = Guid.NewGuid(),
            PurchaseOrderId = purchaseOrderId,
            InventoryItemId = item.Id,
            InventoryItem = item,
            QuantityOrdered = quantityOrdered,
            QuantityReserved = 0,
            CreatedBy = user.Trim(),
            CreatedAt = occurredAt
        };
    }
}
