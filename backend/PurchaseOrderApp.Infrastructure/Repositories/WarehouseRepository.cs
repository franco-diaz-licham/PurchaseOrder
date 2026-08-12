using Microsoft.EntityFrameworkCore;
using PurchaseOrderApp.Application.Models;
using PurchaseOrderApp.Application.Ports;
using PurchaseOrderApp.Domain.Entities;
using PurchaseOrderApp.Domain.ValueObjects;

namespace PurchaseOrderApp.Infrastructure.Repositories;

public sealed class WarehouseRepository(DatabaseContext db) : IWarehouseRepository
{
    public Task<List<WarehouseResponse>> ListResponsesAsync(CancellationToken cancellationToken)
    {
        return db.Warehouses
            .AsNoTracking()
            .OrderBy(warehouse => warehouse.Code)
            .Select(warehouse => new WarehouseResponse(
                warehouse.Id.Value,
                warehouse.Code,
                warehouse.Name))
            .ToListAsync(cancellationToken);
    }

    public Task<Warehouse?> GetAsync(WarehouseId warehouseId, CancellationToken cancellationToken)
    {
        return db.Warehouses.SingleOrDefaultAsync(warehouse => warehouse.Id == warehouseId, cancellationToken);
    }

}
