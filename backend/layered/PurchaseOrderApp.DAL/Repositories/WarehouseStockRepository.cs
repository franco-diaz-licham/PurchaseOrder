using Microsoft.EntityFrameworkCore;
using PurchaseOrderApp.BL.Enums;
using PurchaseOrderApp.BL.Models;
using PurchaseOrderApp.BL.Ports;
using PurchaseOrderApp.BL.Responses;

namespace PurchaseOrderApp.DAL.Repositories;

public sealed class WarehouseStockRepository(PurchaseOrderDbContext db) : IWarehouseStockRepository
{
    public async Task<List<WarehouseStockResponse>> ListResponsesAsync(Guid warehouseId, CancellationToken cancellationToken)
    {
        var stockBalances = await db.WarehouseStock
            .AsNoTracking()
            .Where(stock => stock.WarehouseId == warehouseId)
            .OrderBy(stock => stock.InventoryItemId)
            .ToListAsync(cancellationToken);

        var activeReservations = await db.StockReservations
            .AsNoTracking()
            .Where(reservation => reservation.WarehouseId == warehouseId && reservation.Status == ReservationStatus.Active)
            .Select(reservation => new {
                reservation.InventoryItemId,
                reservation.QuantityReserved
            })
            .ToListAsync(cancellationToken);

        var activeReservedByItem = activeReservations
            .GroupBy(reservation => reservation.InventoryItemId)
            .ToDictionary(group => group.Key, group => group.Sum(reservation => reservation.QuantityReserved));

        return stockBalances.Select(stock => {
            var activeReservedQuantity = activeReservedByItem.GetValueOrDefault(stock.InventoryItemId, 0);
            return new WarehouseStockResponse(
                stock.WarehouseId,
                stock.InventoryItemId,
                stock.OnHandQuantity,
                activeReservedQuantity,
                stock.OnHandQuantity - activeReservedQuantity);
        }).ToList();
    }

    public Task<WarehouseStock?> GetForUpdateAsync(Guid warehouseId, Guid inventoryItemId, CancellationToken cancellationToken)
    {
        return db.WarehouseStock
            .FromSqlInterpolated($"""
                SELECT *
                FROM warehouse_stock
                WHERE warehouse_id = {warehouseId} AND inventory_item_id = {inventoryItemId}
                FOR UPDATE
                """)
            .SingleOrDefaultAsync(cancellationToken);
    }
}
