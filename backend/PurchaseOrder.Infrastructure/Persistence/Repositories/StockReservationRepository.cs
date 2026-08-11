using Microsoft.EntityFrameworkCore;
using PurchaseOrder.Application.Ports;
using PurchaseOrder.Domain.Entities;
using PurchaseOrder.Domain.Enums;
using PurchaseOrder.Domain.ValueObjects;

namespace PurchaseOrder.Infrastructure.Persistence.Repositories;

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
