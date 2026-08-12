using Microsoft.EntityFrameworkCore;
using PurchaseOrderApp.Application.Ports;
using PurchaseOrderApp.Domain.Entities;
using PurchaseOrderApp.Domain.ValueObjects;

namespace PurchaseOrderApp.Infrastructure.Repositories;

public sealed class WarehouseStockRepository(DatabaseContext db) : IWarehouseStockRepository
{
    public Task<WarehouseStock?> GetAsync(WarehouseId warehouseId, InventoryItemId inventoryItemId, CancellationToken cancellationToken)
    {
        return db.WarehouseStock.SingleOrDefaultAsync(stock =>
            stock.WarehouseId == warehouseId &&
            stock.InventoryItemId == inventoryItemId,
            cancellationToken);
    }

    public Task<List<WarehouseStock>> ListAsync(WarehouseId warehouseId, CancellationToken cancellationToken)
    {
        return db.WarehouseStock
            .AsNoTracking()
            .Where(stock => stock.WarehouseId == warehouseId)
            .OrderBy(stock => stock.InventoryItemId)
            .ToListAsync(cancellationToken);
    }

    public Task<WarehouseStock?> GetForUpdateAsync(WarehouseId warehouseId, InventoryItemId inventoryItemId, CancellationToken cancellationToken)
    {
        return db.WarehouseStock
            .FromSqlInterpolated($"""
                SELECT *
                FROM warehouse_stock
                WHERE "WarehouseId" = {warehouseId.Value} AND "InventoryItemId" = {inventoryItemId.Value}
                FOR UPDATE
                """)
            .SingleOrDefaultAsync(cancellationToken);
    }
}
