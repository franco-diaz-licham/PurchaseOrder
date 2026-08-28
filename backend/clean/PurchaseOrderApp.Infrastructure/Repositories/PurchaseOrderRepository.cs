using Microsoft.EntityFrameworkCore;
using PurchaseOrderApp.Application.Models;
using PurchaseOrderApp.Application.Ports;
using PurchaseOrderApp.Domain.Entities;
using PurchaseOrderApp.Domain.Enums;
using PurchaseOrderApp.Domain.ValueObjects;

namespace PurchaseOrderApp.Infrastructure.Repositories;

public sealed class PurchaseOrderRepository(DatabaseContext db) : IPurchaseOrderRepository
{
    private const decimal GstRate = 0.10m;

    public Task<PurchaseOrder?> GetAsync(PurchaseOrderId purchaseOrderId, CancellationToken cancellationToken)
    {
        return db.PurchaseOrders
            .AsSplitQuery()
            .Include(order => order.Lines)
                .ThenInclude(line => line.InventoryItem)
            .SingleOrDefaultAsync(order => order.Id == purchaseOrderId, cancellationToken);
    }

    public Task<PurchaseOrder?> GetByLineIdAsync(PurchaseOrderLineId purchaseOrderLineId, CancellationToken cancellationToken)
    {
        return db.PurchaseOrders
            .AsSplitQuery()
            .Include(order => order.Lines)
                .ThenInclude(line => line.InventoryItem)
            .SingleOrDefaultAsync(order => order.Lines.Any(line => line.Id == purchaseOrderLineId), cancellationToken);
    }

    public async Task<PurchaseOrderResponse?> GetResponseAsync(PurchaseOrderId purchaseOrderId, CancellationToken cancellationToken)
    {
        var header = await db.PurchaseOrders
            .AsNoTracking()
            .Where(order => order.Id == purchaseOrderId)
            .Select(order => new PurchaseOrderHeaderProjection(
                order.Id,
                order.PurchaseOrderNumber,
                order.WarehouseId,
                order.Status))
            .SingleOrDefaultAsync(cancellationToken);

        if (header is null) return null;

        var lineRows = await db.PurchaseOrderLines
            .AsNoTracking()
            .Where(line => line.PurchaseOrderId == purchaseOrderId)
            .OrderBy(line => line.Id)
            .Select(line => new PurchaseOrderLineProjection(
                line.Id,
                line.InventoryItemId,
                line.QuantityOrdered.Value,
                line.QuantityReserved.Value,
                line.QuantityRemaining.Value,
                line.InventoryItem.StandardCost.Value))
            .ToListAsync(cancellationToken);

        var lines = lineRows
            .Select(line => {
                var lineAmount = RoundMoney(line.QuantityOrdered * line.UnitCost);

                return new PurchaseOrderLineResponse(
                    line.PurchaseOrderLineId.Value,
                    line.InventoryItemId.Value,
                    line.QuantityOrdered,
                    line.QuantityReserved,
                    line.QuantityRemaining,
                    line.UnitCost,
                    lineAmount);
            })
            .ToList();

        var subtotalAmount = RoundMoney(lines.Sum(line => line.LineAmount));
        var gstAmount = RoundMoney(subtotalAmount * GstRate);
        var totalAmount = RoundMoney(subtotalAmount + gstAmount);

        return new PurchaseOrderResponse(
            header.PurchaseOrderId.Value,
            header.PurchaseOrderNumber,
            header.WarehouseId.Value,
            header.Status.ToString(),
            subtotalAmount,
            gstAmount,
            totalAmount,
            lines);
    }

    public async Task<List<PurchaseOrderSummaryResponse>> ListSummariesAsync(CancellationToken cancellationToken)
    {
        var headers = await db.PurchaseOrders
            .AsNoTracking()
            .OrderByDescending(order => order.CreatedAt)
            .Select(order => new PurchaseOrderHeaderProjection(
                order.Id,
                order.PurchaseOrderNumber,
                order.WarehouseId,
                order.Status))
            .ToListAsync(cancellationToken);

        var lineRows = await db.PurchaseOrderLines
            .AsNoTracking()
            .Select(line => new PurchaseOrderSummaryLineProjection(
                line.PurchaseOrderId,
                line.QuantityOrdered.Value,
                line.QuantityReserved.Value,
                line.QuantityRemaining.Value,
                line.InventoryItem.StandardCost.Value))
            .ToListAsync(cancellationToken);

        var linesByPurchaseOrder = lineRows
            .GroupBy(line => line.PurchaseOrderId)
            .ToDictionary(lineGroup => lineGroup.Key, lineGroup => lineGroup.ToList());

        return headers
            .Select(header => {
                linesByPurchaseOrder.TryGetValue(header.PurchaseOrderId, out var lines);
                lines ??= [];
                var subtotalAmount = RoundMoney(lines.Sum(line => line.QuantityOrdered * line.UnitCost));
                var gstAmount = RoundMoney(subtotalAmount * GstRate);
                var totalAmount = RoundMoney(subtotalAmount + gstAmount);

                return new PurchaseOrderSummaryResponse(
                    header.PurchaseOrderId.Value,
                    header.PurchaseOrderNumber,
                    header.WarehouseId.Value,
                    header.Status.ToString(),
                    lines.Count,
                    lines.Sum(line => line.QuantityOrdered),
                    lines.Sum(line => line.QuantityReserved),
                    lines.Sum(line => line.QuantityRemaining),
                    subtotalAmount,
                    gstAmount,
                    totalAmount);
            })
            .ToList();
    }

    public async Task AddAsync(PurchaseOrder purchaseOrder, CancellationToken cancellationToken)
    {
        await db.PurchaseOrders.AddAsync(purchaseOrder, cancellationToken);
    }

    private static decimal RoundMoney(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    private sealed record PurchaseOrderHeaderProjection(
        PurchaseOrderId PurchaseOrderId,
        string PurchaseOrderNumber,
        WarehouseId WarehouseId,
        PurchaseOrderStatus Status);

    private sealed record PurchaseOrderLineProjection(
        PurchaseOrderLineId PurchaseOrderLineId,
        InventoryItemId InventoryItemId,
        decimal QuantityOrdered,
        decimal QuantityReserved,
        decimal QuantityRemaining,
        decimal UnitCost);

    private sealed record PurchaseOrderSummaryLineProjection(
        PurchaseOrderId PurchaseOrderId,
        decimal QuantityOrdered,
        decimal QuantityReserved,
        decimal QuantityRemaining,
        decimal UnitCost);
}
