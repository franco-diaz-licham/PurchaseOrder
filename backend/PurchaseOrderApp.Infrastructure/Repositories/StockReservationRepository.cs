using Microsoft.EntityFrameworkCore;
using PurchaseOrderApp.Application.Models;
using PurchaseOrderApp.Application.Ports;
using PurchaseOrderApp.Domain.Entities;
using PurchaseOrderApp.Domain.Enums;
using PurchaseOrderApp.Domain.ValueObjects;

namespace PurchaseOrderApp.Infrastructure.Repositories;

public sealed class StockReservationRepository(DatabaseContext db) : IStockReservationRepository
{
    public Task<StockReservation?> GetAsync(StockReservationId stockReservationId, CancellationToken cancellationToken)
    {
        return db.StockReservations.SingleOrDefaultAsync(reservation => reservation.Id == stockReservationId, cancellationToken);
    }

    public async Task<List<ReservationResponse>> ListResponsesAsync(WarehouseId? warehouseId, ReservationStatus? status, CancellationToken cancellationToken)
    {
        var query = db.StockReservations.AsNoTracking();
        if (warehouseId is not null) query = query.Where(reservation => reservation.WarehouseId == warehouseId.Value);
        if (status is not null) query = query.Where(reservation => reservation.Status == status.Value);

        var reservations = await query
            .OrderBy(reservation => reservation.Status)
            .ThenBy(reservation => reservation.Id)
            .Select(reservation => new ReservationProjection(
                reservation.Id,
                reservation.PurchaseOrderLineId,
                reservation.WarehouseId,
                reservation.InventoryItemId,
                reservation.QuantityReserved.Value,
                reservation.UnitCostSnapshot.Value,
                reservation.Status,
                reservation.CreatedBy,
                reservation.CreatedAt))
            .ToListAsync(cancellationToken);

        return reservations
            .Select(reservation => new ReservationResponse(
                reservation.StockReservationId.Value,
                reservation.PurchaseOrderLineId.Value,
                reservation.WarehouseId.Value,
                reservation.InventoryItemId.Value,
                reservation.QuantityReserved,
                reservation.UnitCostSnapshot,
                reservation.Status.ToString(),
                reservation.CreatedBy,
                reservation.CreatedAt))
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

    private sealed record ReservationProjection(
        StockReservationId StockReservationId,
        PurchaseOrderLineId PurchaseOrderLineId,
        WarehouseId WarehouseId,
        InventoryItemId InventoryItemId,
        decimal QuantityReserved,
        decimal UnitCostSnapshot,
        ReservationStatus Status,
        string CreatedBy,
        DateTimeOffset CreatedAt);
}
