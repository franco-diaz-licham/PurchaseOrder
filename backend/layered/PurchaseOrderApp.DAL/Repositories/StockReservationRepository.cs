using Microsoft.EntityFrameworkCore;
using PurchaseOrderApp.BL.Enums;
using PurchaseOrderApp.BL.Models;
using PurchaseOrderApp.BL.Ports;
using PurchaseOrderApp.BL.Responses;

namespace PurchaseOrderApp.DAL.Repositories;

public sealed class StockReservationRepository(PurchaseOrderDbContext db) : IStockReservationRepository
{
    public Task<StockReservation?> GetAsync(Guid stockReservationId, CancellationToken cancellationToken)
    {
        return db.StockReservations.SingleOrDefaultAsync(reservation => reservation.Id == stockReservationId, cancellationToken);
    }

    public Task<List<ReservationResponse>> ListResponsesAsync(CancellationToken cancellationToken)
    {
        return db.StockReservations
            .AsNoTracking()
            .OrderBy(reservation => reservation.Status)
            .ThenBy(reservation => reservation.Id)
            .Select(reservation => new ReservationResponse(
                reservation.Id,
                reservation.PurchaseOrderLineId,
                reservation.WarehouseId,
                reservation.InventoryItemId,
                reservation.QuantityReserved,
                reservation.UnitCostSnapshot,
                reservation.Status.ToString(),
                reservation.CreatedBy,
                reservation.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetActiveReservedQuantityAsync(Guid warehouseId, Guid inventoryItemId, CancellationToken cancellationToken)
    {
        return await db.StockReservations
            .AsNoTracking()
            .Where(reservation =>
                reservation.WarehouseId == warehouseId &&
                reservation.InventoryItemId == inventoryItemId &&
                reservation.Status == ReservationStatus.Active)
            .SumAsync(reservation => reservation.QuantityReserved, cancellationToken);
    }

    public Task<List<StockReservation>> ListActiveByLineAsync(Guid purchaseOrderLineId, CancellationToken cancellationToken)
    {
        return db.StockReservations
            .Where(reservation => reservation.PurchaseOrderLineId == purchaseOrderLineId && reservation.Status == ReservationStatus.Active)
            .OrderBy(reservation => reservation.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(StockReservation stockReservation, CancellationToken cancellationToken)
    {
        await db.StockReservations.AddAsync(stockReservation, cancellationToken);
    }
}
