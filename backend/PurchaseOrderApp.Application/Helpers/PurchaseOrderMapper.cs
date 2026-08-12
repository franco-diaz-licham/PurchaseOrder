using PurchaseOrderApp.Application.Models;
using PurchaseOrderApp.Domain.Entities;

namespace PurchaseOrderApp.Application.Helpers;

public static class PurchaseOrderMapper
{
    private const decimal GstRate = 0.10m;

    public static PurchaseOrderResponse ToResponse(PurchaseOrder purchaseOrder)
    {
        var lines = purchaseOrder.Lines
            .Select(ToLineResponse)
            .ToList();
        var subtotalAmount = RoundMoney(lines.Sum(line => line.LineAmount));
        var gstAmount = RoundMoney(subtotalAmount * GstRate);
        var totalAmount = RoundMoney(subtotalAmount + gstAmount);

        return new PurchaseOrderResponse(
            purchaseOrder.Id.Value,
            purchaseOrder.PurchaseOrderNumber,
            purchaseOrder.WarehouseId.Value,
            purchaseOrder.Status.ToString(),
            subtotalAmount,
            gstAmount,
            totalAmount,
            lines);
    }

    private static PurchaseOrderLineResponse ToLineResponse(PurchaseOrderLine line)
    {
        var unitCost = line.InventoryItem.StandardCost.Value;
        var lineAmount = RoundMoney(line.QuantityOrdered.Value * unitCost);

        return new PurchaseOrderLineResponse(
            line.Id.Value,
            line.InventoryItemId.Value,
            line.QuantityOrdered.Value,
            line.QuantityReserved.Value,
            line.QuantityRemaining.Value,
            unitCost,
            lineAmount);
    }

    private static decimal RoundMoney(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
