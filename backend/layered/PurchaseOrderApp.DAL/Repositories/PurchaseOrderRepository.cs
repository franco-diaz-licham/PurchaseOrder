using Microsoft.EntityFrameworkCore;
using PurchaseOrderApp.BL.Models;
using PurchaseOrderApp.BL.Ports;
using PurchaseOrderApp.BL.Responses;

namespace PurchaseOrderApp.DAL.Repositories;

public sealed class PurchaseOrderRepository(PurchaseOrderDbContext db) : IPurchaseOrderRepository
{
    private const decimal GstRate = 0.10m;

    public Task<PurchaseOrder?> GetAsync(Guid purchaseOrderId, CancellationToken cancellationToken)
    {
        return db.PurchaseOrders
            .AsSplitQuery()
            .Include(order => order.Lines)
                .ThenInclude(line => line.InventoryItem)
            .SingleOrDefaultAsync(order => order.Id == purchaseOrderId, cancellationToken);
    }

    public Task<PurchaseOrder?> GetByLineIdAsync(Guid purchaseOrderLineId, CancellationToken cancellationToken)
    {
        return db.PurchaseOrders
            .AsSplitQuery()
            .Include(order => order.Lines)
                .ThenInclude(line => line.InventoryItem)
            .SingleOrDefaultAsync(order => order.Lines.Any(line => line.Id == purchaseOrderLineId), cancellationToken);
    }

    public async Task<PurchaseOrderResponse?> GetResponseAsync(Guid purchaseOrderId, CancellationToken cancellationToken)
    {
        var order = await db.PurchaseOrders
            .AsNoTracking()
            .Where(order => order.Id == purchaseOrderId)
            .Select(order => new {
                order.Id,
                order.PurchaseOrderNumber,
                order.WarehouseId,
                order.Status
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (order is null) return null;

        var lineRows = await db.PurchaseOrderLines
            .AsNoTracking()
            .Where(line => line.PurchaseOrderId == purchaseOrderId)
            .OrderBy(line => line.Id)
            .Select(line => new {
                line.Id,
                line.InventoryItemId,
                line.QuantityOrdered,
                line.QuantityReserved,
                UnitCost = line.InventoryItem!.StandardCost
            })
            .ToListAsync(cancellationToken);

        var lines = lineRows
            .Select(line => {
                var lineAmount = RoundMoney(line.QuantityOrdered * line.UnitCost);
                return new PurchaseOrderLineResponse(
                    line.Id,
                    line.InventoryItemId,
                    line.QuantityOrdered,
                    line.QuantityReserved,
                    line.QuantityOrdered - line.QuantityReserved,
                    line.UnitCost,
                    lineAmount);
            })
            .ToList();

        var subtotal = RoundMoney(lines.Sum(line => line.LineAmount));
        var gst = RoundMoney(subtotal * GstRate);

        return new PurchaseOrderResponse(order.Id, order.PurchaseOrderNumber, order.WarehouseId, order.Status.ToString(), subtotal, gst, RoundMoney(subtotal + gst), lines);
    }

    public async Task<List<PurchaseOrderSummaryResponse>> ListSummariesAsync(CancellationToken cancellationToken)
    {
        var headers = await db.PurchaseOrders
            .AsNoTracking()
            .OrderByDescending(order => order.CreatedAt)
            .Select(order => new {
                order.Id,
                order.PurchaseOrderNumber,
                order.WarehouseId,
                order.Status
            })
            .ToListAsync(cancellationToken);

        var lineRows = await db.PurchaseOrderLines
            .AsNoTracking()
            .Select(line => new {
                line.PurchaseOrderId,
                line.QuantityOrdered,
                line.QuantityReserved,
                UnitCost = line.InventoryItem!.StandardCost
            })
            .ToListAsync(cancellationToken);

        var linesByOrder = lineRows
            .GroupBy(line => line.PurchaseOrderId)
            .ToDictionary(group => group.Key, group => group.ToList());

        return headers.Select(header => {
            var lines = linesByOrder.GetValueOrDefault(header.Id, []);
            var subtotal = RoundMoney(lines.Sum(line => line.QuantityOrdered * line.UnitCost));
            var gst = RoundMoney(subtotal * GstRate);
            return new PurchaseOrderSummaryResponse(
                header.Id,
                header.PurchaseOrderNumber,
                header.WarehouseId,
                header.Status.ToString(),
                lines.Count,
                lines.Sum(line => line.QuantityOrdered),
                lines.Sum(line => line.QuantityReserved),
                lines.Sum(line => line.QuantityOrdered - line.QuantityReserved),
                subtotal,
                gst,
                RoundMoney(subtotal + gst));
        }).ToList();
    }

    public async Task AddAsync(PurchaseOrder purchaseOrder, CancellationToken cancellationToken)
    {
        await db.PurchaseOrders.AddAsync(purchaseOrder, cancellationToken);
    }

    private static decimal RoundMoney(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
