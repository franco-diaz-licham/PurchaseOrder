using Microsoft.EntityFrameworkCore;
using PurchaseOrderApp.Application.Ports;
using PurchaseOrderApp.Domain.Entities;
using PurchaseOrderApp.Domain.ValueObjects;

namespace PurchaseOrderApp.Infrastructure.Repositories;

public sealed class WarehouseRepository(DatabaseContext db) : IWarehouseRepository
{
    public Task<List<Warehouse>> ListAsync(CancellationToken cancellationToken)
    {
        return db.Warehouses
            .AsNoTracking()
            .OrderBy(warehouse => warehouse.Code)
            .ToListAsync(cancellationToken);
    }

    public Task<Warehouse?> GetAsync(WarehouseId warehouseId, CancellationToken cancellationToken)
    {
        return db.Warehouses.SingleOrDefaultAsync(warehouse => warehouse.Id == warehouseId, cancellationToken);
    }

    public async Task AddAsync(Warehouse warehouse, CancellationToken cancellationToken)
    {
        await db.Warehouses.AddAsync(warehouse, cancellationToken);
    }
}
