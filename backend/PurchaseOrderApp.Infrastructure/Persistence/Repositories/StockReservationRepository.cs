using Microsoft.EntityFrameworkCore;
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
}
