using Microsoft.EntityFrameworkCore;
using PurchaseOrder.Application.Ports;
using PurchaseOrder.Domain.Entities;
using PurchaseOrder.Domain.ValueObjects;

namespace PurchaseOrder.Infrastructure.Persistence.Repositories;

public sealed class WarehouseRepository(DatabaseContext db) : IWarehouseRepository
{
    public Task<Warehouse?> GetAsync(WarehouseId warehouseId, CancellationToken cancellationToken)
    {
        return db.Warehouses.SingleOrDefaultAsync(warehouse => warehouse.Id == warehouseId, cancellationToken);
    }

    public async Task AddAsync(Warehouse warehouse, CancellationToken cancellationToken)
    {
        await db.Warehouses.AddAsync(warehouse, cancellationToken);
    }
}
