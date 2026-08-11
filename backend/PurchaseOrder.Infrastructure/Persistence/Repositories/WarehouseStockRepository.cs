using Microsoft.EntityFrameworkCore;
using PurchaseOrder.Application.Ports;
using PurchaseOrder.Domain.Entities;
using PurchaseOrder.Domain.ValueObjects;

namespace PurchaseOrder.Infrastructure.Persistence.Repositories;

public sealed class WarehouseStockRepository(DatabaseContext db) : IWarehouseStockRepository
{
    public Task<WarehouseStock?> GetAsync(WarehouseId warehouseId, InventoryItemId inventoryItemId, CancellationToken cancellationToken)
    {
        return db.WarehouseStock.SingleOrDefaultAsync(stock =>
            stock.WarehouseId == warehouseId &&
            stock.InventoryItemId == inventoryItemId,
            cancellationToken);
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

    public async Task AddAsync(WarehouseStock warehouseStock, CancellationToken cancellationToken)
    {
        await db.WarehouseStock.AddAsync(warehouseStock, cancellationToken);
    }
}
