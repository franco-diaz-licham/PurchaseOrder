using PurchaseOrderApp.BL.Models;
using PurchaseOrderApp.BL.Responses;

namespace PurchaseOrderApp.BL.Mappers;

public static class ResponseMapper
{
    private const decimal GstRate = 0.10m;

    public static PurchaseOrderResponse ToPurchaseOrderResponse(PurchaseOrder purchaseOrder)
    {
        var lines = purchaseOrder.Lines
            .OrderBy(line => line.Id)
            .Select(line => {
                var unitCost = line.InventoryItem?.StandardCost ?? 0;
                var lineAmount = RoundMoney(line.QuantityOrdered * unitCost);

                return new PurchaseOrderLineResponse(
                    line.Id,
                    line.InventoryItemId,
                    line.QuantityOrdered,
                    line.QuantityReserved,
                    line.QuantityRemaining,
                    unitCost,
                    lineAmount);
            })
            .ToList();

        var subtotal = RoundMoney(lines.Sum(line => line.LineAmount));
        var gst = RoundMoney(subtotal * GstRate);

        return new PurchaseOrderResponse(
            purchaseOrder.Id,
            purchaseOrder.PurchaseOrderNumber,
            purchaseOrder.WarehouseId,
            purchaseOrder.Status.ToString(),
            subtotal,
            gst,
            RoundMoney(subtotal + gst),
            lines);
    }

    public static ReservationResponse ToReservationResponse(StockReservation reservation)
    {
        return new ReservationResponse(
            reservation.Id,
            reservation.PurchaseOrderLineId,
            reservation.WarehouseId,
            reservation.InventoryItemId,
            reservation.QuantityReserved,
            reservation.UnitCostSnapshot,
            reservation.Status.ToString(),
            reservation.CreatedBy,
            reservation.CreatedAt);
    }

    private static decimal RoundMoney(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
