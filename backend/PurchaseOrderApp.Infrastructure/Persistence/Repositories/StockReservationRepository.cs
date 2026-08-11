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

    public Task<ReservationResponse?> GetResponseAsync(StockReservationId stockReservationId, CancellationToken cancellationToken)
    {
        return ProjectReservations(db.StockReservations.AsNoTracking())
            .SingleOrDefaultAsync(reservation => reservation.StockReservationId == stockReservationId.Value, cancellationToken);
    }

    public Task<List<ReservationResponse>> ListResponsesAsync(WarehouseId? warehouseId, ReservationStatus? status, CancellationToken cancellationToken)
    {
        var query = db.StockReservations.AsNoTracking();

        if (warehouseId is not null)
        {
            query = query.Where(reservation => reservation.WarehouseId == warehouseId.Value);
        }

        if (status is not null)
        {
            query = query.Where(reservation => reservation.Status == status.Value);
        }

        return ProjectReservations(query)
            .OrderBy(reservation => reservation.Status)
            .ThenBy(reservation => reservation.StockReservationId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Quantity> GetActiveReservedQuantityAsync(WarehouseId warehouseId, InventoryItemId inventoryItemId, CancellationToken cancellationToken)
    {
        var quantity = await db.StockReservations
            .Where(reservation =>
                reservation.WarehouseId == warehouseId &&
                reservation.InventoryItemId == inventoryItemId &&
                reservation.Status == ReservationStatus.Active)
            .SumAsync(reservation => reservation.QuantityReserved.Value, cancellationToken);

        return new Quantity(quantity);
    }

    public async Task AddAsync(StockReservation stockReservation, CancellationToken cancellationToken)
    {
        await db.StockReservations.AddAsync(stockReservation, cancellationToken);
    }

    private static IQueryable<ReservationResponse> ProjectReservations(IQueryable<StockReservation> query)
    {
        return query.Select(reservation => new ReservationResponse(
            reservation.Id.Value,
            reservation.PurchaseOrderLineId.Value,
            reservation.WarehouseId.Value,
            reservation.InventoryItemId.Value,
            reservation.QuantityReserved.Value,
            reservation.UnitCostSnapshot.Value,
            reservation.Status.ToString()));
    }
}
