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

    public async Task<PurchaseOrderResponse?> GetResponseAsync(PurchaseOrderId purchaseOrderId, CancellationToken cancellationToken)
    {
        var purchaseOrder = await db.PurchaseOrders
            .AsNoTracking()
            .Include(order => order.Lines)
            .SingleOrDefaultAsync(order => order.Id == purchaseOrderId, cancellationToken);

        return purchaseOrder is null ? null : ToResponse(purchaseOrder);
    }

    public async Task<List<PurchaseOrderResponse>> ListResponsesAsync(CancellationToken cancellationToken)
    {
        var purchaseOrders = await db.PurchaseOrders
            .AsNoTracking()
            .Include(order => order.Lines)
            .OrderBy(order => order.PurchaseOrderNumber)
            .ToListAsync(cancellationToken);

        return purchaseOrders
            .Select(ToResponse)
            .ToList();
    }

    public async Task<List<PurchaseOrderSummaryResponse>> ListSummariesAsync(CancellationToken cancellationToken)
    {
        var purchaseOrders = await db.PurchaseOrders
            .AsNoTracking()
            .Include(order => order.Lines)
            .OrderBy(order => order.PurchaseOrderNumber)
            .ToListAsync(cancellationToken);

        return purchaseOrders
            .Select(order => new PurchaseOrderSummaryResponse(
                order.Id.Value,
                order.PurchaseOrderNumber,
                order.WarehouseId.Value,
                order.Status.ToString(),
                order.Lines.Count,
                order.Lines.Sum(line => line.QuantityOrdered.Value),
                order.Lines.Sum(line => line.QuantityReserved.Value),
                order.Lines.Sum(line => line.QuantityRemaining.Value)))
            .ToList();
    }

    public async Task AddAsync(PurchaseOrder purchaseOrder, CancellationToken cancellationToken)
    {
        await db.PurchaseOrders.AddAsync(purchaseOrder, cancellationToken);
    }

    public async Task<List<ApprovedPurchaseOrderLineResponse>> ListApprovedOutstandingLinesAsync(WarehouseId warehouseId, CancellationToken cancellationToken)
    {
        var approvedOrders = await db.PurchaseOrders
            .AsNoTracking()
            .Include(order => order.Warehouse)
            .Include(order => order.Lines)
                .ThenInclude(line => line.InventoryItem)
            .Where(order => order.WarehouseId == warehouseId)
            .Where(order => order.Status == PurchaseOrderStatus.Approved)
            .OrderBy(line => line.PurchaseOrderNumber)
            .ToListAsync(cancellationToken);

        return approvedOrders
            .SelectMany(order => order.Lines
                .Where(line => !line.QuantityRemaining.IsZero)
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
                    line.QuantityRemaining.Value)))
            .OrderBy(line => line.PurchaseOrderNumber)
            .ThenBy(line => line.Sku)
            .ToList();
    }

    private static PurchaseOrderResponse ToResponse(PurchaseOrder order)
    {
        return new PurchaseOrderResponse(
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
                    line.QuantityRemaining.Value))
                .ToList());
    }
}
