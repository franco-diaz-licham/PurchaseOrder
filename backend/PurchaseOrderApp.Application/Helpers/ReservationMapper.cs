using PurchaseOrderApp.Application.Models;
using PurchaseOrderApp.Domain.Entities;

namespace PurchaseOrderApp.Application.Helpers;

public static class ReservationMapper
{
    public static ReservationResponse ToResponse(StockReservation reservation)
    {
        return new ReservationResponse(
            reservation.Id.Value,
            reservation.PurchaseOrderLineId.Value,
            reservation.WarehouseId.Value,
            reservation.InventoryItemId.Value,
            reservation.QuantityReserved.Value,
            reservation.UnitCostSnapshot.Value,
            reservation.Status.ToString(),
            reservation.CreatedBy,
            reservation.CreatedAt);
    }
}
