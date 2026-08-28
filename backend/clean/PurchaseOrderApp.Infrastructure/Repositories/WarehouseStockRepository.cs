using Microsoft.EntityFrameworkCore;
using PurchaseOrderApp.Application.Models;
using PurchaseOrderApp.Application.Ports;
using PurchaseOrderApp.Domain.Entities;
using PurchaseOrderApp.Domain.Enums;
using PurchaseOrderApp.Domain.ValueObjects;

namespace PurchaseOrderApp.Infrastructure.Repositories;

public sealed class WarehouseStockRepository(DatabaseContext db) : IWarehouseStockRepository
{
    public async Task<List<WarehouseStockResponse>> ListResponsesAsync(WarehouseId warehouseId, CancellationToken cancellationToken)
    {
        var stockBalances = await db.WarehouseStock
            .AsNoTracking()
            .Where(stock => stock.WarehouseId == warehouseId)
            .OrderBy(stock => stock.InventoryItemId)
            .ToListAsync(cancellationToken);

        var activeReservations = await db.StockReservations
            .AsNoTracking()
            .Where(reservation =>
                reservation.WarehouseId == warehouseId &&
                reservation.Status == ReservationStatus.Active)
            .Select(reservation => new {
                reservation.InventoryItemId,
                QuantityReserved = reservation.QuantityReserved.Value
            })
            .ToListAsync(cancellationToken);

        var activeReservedByItem = activeReservations
            .GroupBy(reservation => reservation.InventoryItemId)
            .ToDictionary(
                reservationGroup => reservationGroup.Key,
                reservationGroup => new Quantity(reservationGroup.Sum(reservation => reservation.QuantityReserved)));

        return stockBalances
            .Select(stock => {
                if (!activeReservedByItem.TryGetValue(stock.InventoryItemId, out var activeReservedQuantity)) {
                    activeReservedQuantity = Quantity.Zero;
                }

                var availableQuantity = stock.CalculateAvailableQuantity(activeReservedQuantity);

                return new WarehouseStockResponse(
                    stock.WarehouseId.Value,
                    stock.InventoryItemId.Value,
                    stock.OnHandQuantity.Value,
                    activeReservedQuantity.Value,
                    availableQuantity.Value);
            })
            .ToList();
    }

    public Task<WarehouseStock?> GetForUpdateAsync(WarehouseId warehouseId, InventoryItemId inventoryItemId, CancellationToken cancellationToken)
    {
        return db.WarehouseStock
            .FromSqlInterpolated($"""
                SELECT *
                FROM warehouse_stock
                WHERE warehouse_id = {warehouseId.Value} AND inventory_item_id = {inventoryItemId.Value}
                FOR UPDATE
                """)
            .SingleOrDefaultAsync(cancellationToken);
    }
}
