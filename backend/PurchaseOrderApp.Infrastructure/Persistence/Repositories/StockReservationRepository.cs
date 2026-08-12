using Microsoft.EntityFrameworkCore;
using PurchaseOrderApp.Application.Models;
using PurchaseOrderApp.Application.Ports;
using PurchaseOrderApp.Domain.Entities;
using PurchaseOrderApp.Domain.Enums;
using PurchaseOrderApp.Domain.ValueObjects;

namespace PurchaseOrderApp.Infrastructure.Persistence.Repositories;

public sealed class StockReservationRepository(DatabaseContext db) : IStockReservationRepository
{
    public Task<StockReservation?> GetAsync(StockReservationId stockReservationId, CancellationToken cancellationToken)
    {
        return db.StockReservations.SingleOrDefaultAsync(reservation => reservation.Id == stockReservationId, cancellationToken);
    }

    public async Task<ReservationResponse?> GetResponseAsync(StockReservationId stockReservationId, CancellationToken cancellationToken)
    {
        var reservation = await db.StockReservations
            .AsNoTracking()
            .SingleOrDefaultAsync(reservation => reservation.Id == stockReservationId, cancellationToken);

        return reservation is null ? null : ToResponse(reservation);
    }

    public async Task<List<ReservationResponse>> ListResponsesAsync(WarehouseId? warehouseId, ReservationStatus? status, CancellationToken cancellationToken)
    {
        var query = db.StockReservations.AsNoTracking();
        if (warehouseId is not null) query = query.Where(reservation => reservation.WarehouseId == warehouseId.Value);
        if (status is not null) query = query.Where(reservation => reservation.Status == status.Value);

        var reservations = await query
            .OrderBy(reservation => reservation.Status)
            .ThenBy(reservation => reservation.Id)
            .ToListAsync(cancellationToken);

        return reservations
            .Select(ToResponse)
            .ToList();
    }

    public async Task<Quantity> GetActiveReservedQuantityAsync(WarehouseId warehouseId, InventoryItemId inventoryItemId, CancellationToken cancellationToken)
    {
        var reservations = await db.StockReservations
            .AsNoTracking()
            .Where(reservation =>
                reservation.WarehouseId == warehouseId &&
                reservation.InventoryItemId == inventoryItemId &&
                reservation.Status == ReservationStatus.Active)
            .ToListAsync(cancellationToken);

        return new Quantity(reservations.Sum(reservation => reservation.QuantityReserved.Value));
    }

    public Task<List<StockReservation>> ListActiveByLineAsync(PurchaseOrderLineId purchaseOrderLineId, CancellationToken cancellationToken)
    {
        return db.StockReservations
            .Where(reservation =>
                reservation.PurchaseOrderLineId == purchaseOrderLineId &&
                reservation.Status == ReservationStatus.Active)
            .OrderBy(reservation => reservation.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(StockReservation stockReservation, CancellationToken cancellationToken)
    {
        await db.StockReservations.AddAsync(stockReservation, cancellationToken);
    }

    private static ReservationResponse ToResponse(StockReservation reservation)
    {
        return new ReservationResponse(
            reservation.Id.Value,
            reservation.PurchaseOrderLineId.Value,
            reservation.WarehouseId.Value,
            reservation.InventoryItemId.Value,
            reservation.QuantityReserved.Value,
            reservation.UnitCostSnapshot.Value,
            reservation.Status.ToString());
    }
}
