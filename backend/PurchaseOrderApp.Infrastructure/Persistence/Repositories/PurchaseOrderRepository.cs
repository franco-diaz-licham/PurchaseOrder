using Microsoft.EntityFrameworkCore;
using PurchaseOrderApp.Application.Models;
using PurchaseOrderApp.Application.Ports;
using PurchaseOrderApp.Domain.Entities;
using PurchaseOrderApp.Domain.Enums;
using PurchaseOrderApp.Domain.ValueObjects;

namespace PurchaseOrderApp.Infrastructure.Persistence.Repositories;

public sealed class PurchaseOrderRepository(DatabaseContext db) : IPurchaseOrderRepository
{
    public Task<PurchaseOrder?> GetAsync(PurchaseOrderId purchaseOrderId, CancellationToken cancellationToken)
    {
        return db.PurchaseOrders
            .Include(order => order.Lines)
            .SingleOrDefaultAsync(order => order.Id == purchaseOrderId, cancellationToken);
    }

    public Task<PurchaseOrder?> GetByLineIdAsync(PurchaseOrderLineId purchaseOrderLineId, CancellationToken cancellationToken)
    {
        return db.PurchaseOrders
            .Include(order => order.Lines)
            .SingleOrDefaultAsync(order => order.Lines.Any(line => line.Id == purchaseOrderLineId), cancellationToken);
    }

    public Task<PurchaseOrderResponse?> GetResponseAsync(PurchaseOrderId purchaseOrderId, CancellationToken cancellationToken)
    {
        return ProjectPurchaseOrders(db.PurchaseOrders.AsNoTracking())
            .SingleOrDefaultAsync(order => order.PurchaseOrderId == purchaseOrderId.Value, cancellationToken);
    }

    public Task<List<PurchaseOrderResponse>> ListResponsesAsync(WarehouseId? warehouseId, CancellationToken cancellationToken)
    {
        var query = db.PurchaseOrders.AsNoTracking();

        if (warehouseId is not null)
        {
            query = query.Where(order => order.WarehouseId == warehouseId.Value);
        }

        return ProjectPurchaseOrders(query)
            .OrderBy(order => order.PurchaseOrderNumber)
            .ToListAsync(cancellationToken);
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

    private static IQueryable<PurchaseOrderResponse> ProjectPurchaseOrders(IQueryable<PurchaseOrder> query)
    {
        return query.Select(order => new PurchaseOrderResponse(
            order.Id.Value,
            order.PurchaseOrderNumber,
            order.WarehouseId.Value,
            order.Status.ToString(),
            order.Lines
                .Select(line => new PurchaseOrderLineResponse(
                    line.Id.Value,
                    line.InventoryItemId.Value,
                    line.QuantityOrdered.Value,
                    line.QuantityReserved.Value,
                    line.QuantityOrdered.Value - line.QuantityReserved.Value))
                .ToList()));
    }
}
