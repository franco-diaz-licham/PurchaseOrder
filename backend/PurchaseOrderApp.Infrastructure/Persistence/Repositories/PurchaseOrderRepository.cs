using Microsoft.EntityFrameworkCore;
using PurchaseOrderApp.Application.Models;
using PurchaseOrderApp.Application.Ports;
using PurchaseOrderApp.Domain.Entities;
using PurchaseOrderApp.Domain.Enums;
using PurchaseOrderApp.Domain.ValueObjects;

namespace PurchaseOrderApp.Infrastructure.Persistence.Repositories;

public sealed class PurchaseOrderRepository(DatabaseContext db) : IPurchaseOrderRepository
{
    public Task<PurchaseOrder?> GetByLineIdAsync(PurchaseOrderLineId purchaseOrderLineId, CancellationToken cancellationToken)
    {
        return db.PurchaseOrders
            .Include(order => order.Lines)
            .SingleOrDefaultAsync(order => order.Lines.Any(line => line.Id == purchaseOrderLineId), cancellationToken);
    }

    public async Task AddAsync(PurchaseOrder purchaseOrder, CancellationToken cancellationToken)
    {
        await db.PurchaseOrders.AddAsync(purchaseOrder, cancellationToken);
    }

    public async Task<List<ApprovedPurchaseOrderLineResponse>> ListApprovedOutstandingLinesAsync(WarehouseId warehouseId, CancellationToken cancellationToken)
    {
        var approvedOrders = db.PurchaseOrders
            .AsNoTracking()
            .Where(order => order.WarehouseId == warehouseId)
            .Where(order => order.Status == PurchaseOrderStatus.Approved);

        return await approvedOrders
            .SelectMany(order => order.Lines
                .Where(line => line.QuantityReserved.Value < line.QuantityOrdered.Value)
                .Select(line => new ApprovedPurchaseOrderLineResponse(
                    order.Id.Value,
                    order.PurchaseOrderNumber,
                    line.Id.Value,
                    order.Warehouse.Id.Value,
                    order.Warehouse.Code,
                    order.Warehouse.Name,
                    line.InventoryItem.Id.Value,
                    line.InventoryItem.Sku,
                    line.InventoryItem.Name,
                    line.QuantityOrdered.Value,
                    line.QuantityReserved.Value,
                    line.QuantityOrdered.Value - line.QuantityReserved.Value)))
            .OrderBy(line => line.PurchaseOrderNumber)
            .ThenBy(line => line.Sku)
            .ToListAsync(cancellationToken);
    }
}
