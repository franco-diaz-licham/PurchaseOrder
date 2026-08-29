using Microsoft.EntityFrameworkCore;
using PurchaseOrderApp.BL.Models;
using PurchaseOrderApp.BL.Ports;
using PurchaseOrderApp.BL.Responses;

namespace PurchaseOrderApp.DAL.Repositories;

public sealed class WarehouseRepository(PurchaseOrderDbContext db) : IWarehouseRepository
{
    public Task<Warehouse?> GetAsync(Guid warehouseId, CancellationToken cancellationToken)
    {
        return db.Warehouses.SingleOrDefaultAsync(warehouse => warehouse.Id == warehouseId, cancellationToken);
    }

    public Task<List<WarehouseResponse>> ListResponsesAsync(CancellationToken cancellationToken)
    {
        return db.Warehouses
            .AsNoTracking()
            .OrderBy(warehouse => warehouse.Code)
            .Select(warehouse => new WarehouseResponse(warehouse.Id, warehouse.Code, warehouse.Name))
            .ToListAsync(cancellationToken);
    }
}
