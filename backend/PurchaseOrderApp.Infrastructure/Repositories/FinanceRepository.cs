using Microsoft.EntityFrameworkCore;
using PurchaseOrderApp.Application.Models;
using PurchaseOrderApp.Application.Ports;
using PurchaseOrderApp.Domain.Enums;
using PurchaseOrderApp.Domain.ValueObjects;

namespace PurchaseOrderApp.Infrastructure.Repositories;

public sealed class FinanceRepository(DatabaseContext db) : IFinanceQueryRepository
{
    public async Task<List<WarehouseCommittedStockValueResponse>> ListWarehouseCommittedStockValuesAsync(CancellationToken cancellationToken)
    {
        var activeReservations = await db.StockReservations
            .AsNoTracking()
            .Where(reservation => reservation.Status == ReservationStatus.Active)
            .Select(reservation => new ActiveReservationProjection(
                reservation.Id,
                reservation.PurchaseOrderLineId,
                reservation.WarehouseId,
                reservation.QuantityReserved.Value,
                reservation.UnitCostSnapshot.Value))
            .ToListAsync(cancellationToken);

        var lineIds = activeReservations
            .Select(reservation => reservation.PurchaseOrderLineId)
            .Distinct()
            .ToList();

        var lines = await db.PurchaseOrderLines
            .AsNoTracking()
            .Where(line => lineIds.Contains(line.Id))
            .Select(line => new PurchaseOrderLineProjection(
                line.Id,
                line.PurchaseOrderId,
                line.InventoryItemId,
                line.InventoryItem.Sku,
                line.InventoryItem.Name,
                line.InventoryItem.TrackingMode))
            .ToListAsync(cancellationToken);

        var lineById = lines.ToDictionary(line => line.PurchaseOrderLineId);

        var purchaseOrderIds = lines
            .Select(line => line.PurchaseOrderId)
            .Distinct()
            .ToList();

        var purchaseOrders = await db.PurchaseOrders
            .AsNoTracking()
            .Where(order => purchaseOrderIds.Contains(order.Id))
            .Select(order => new PurchaseOrderProjection(
                order.Id,
                order.PurchaseOrderNumber))
            .ToListAsync(cancellationToken);

        var purchaseOrderById = purchaseOrders.ToDictionary(order => order.PurchaseOrderId);

        var reservationDetails = activeReservations
            .Select(reservation => {
                if (!lineById.TryGetValue(reservation.PurchaseOrderLineId, out var line)) return null;
                if (!purchaseOrderById.TryGetValue(line.PurchaseOrderId, out var purchaseOrder)) return null;
                var committedValue = reservation.QuantityReserved * reservation.UnitCostSnapshot;
                return new ReservationDetailProjection(
                    reservation.WarehouseId,
                    purchaseOrder.PurchaseOrderNumber,
                    line.Sku,
                    new WarehouseCommittedStockReservationResponse(
                        reservation.StockReservationId.Value,
                        purchaseOrder.PurchaseOrderId.Value,
                        purchaseOrder.PurchaseOrderNumber,
                        line.PurchaseOrderLineId.Value,
                        line.InventoryItemId.Value,
                        line.Sku,
                        line.ItemName,
                        line.TrackingMode.ToString(),
                        reservation.QuantityReserved,
                        reservation.UnitCostSnapshot,
                        committedValue));
            })
            .Where(reservation => reservation is not null)
            .Select(reservation => reservation!)
            .ToList();

        var reservationsByWarehouse = activeReservations
            .GroupBy(reservation => reservation.WarehouseId)
            .ToDictionary(group => group.Key, group => group.ToList());

        var reservationDetailsByWarehouse = reservationDetails
            .GroupBy(reservation => reservation.WarehouseId)
            .ToDictionary(group => group.Key, group => group.OrderBy(reservation => reservation.PurchaseOrderNumber).ThenBy(reservation => reservation.Sku).ToList());

        var warehouses = await db.Warehouses
            .AsNoTracking()
            .OrderBy(warehouse => warehouse.Code)
            .Select(warehouse => new WarehouseProjection(
                warehouse.Id,
                warehouse.Code,
                warehouse.Name))
            .ToListAsync(cancellationToken);

        return warehouses
            .Select(warehouse => {
                var reservations = reservationsByWarehouse.GetValueOrDefault(warehouse.WarehouseId, []);
                var details = reservationDetailsByWarehouse.GetValueOrDefault(warehouse.WarehouseId, []);
                return new WarehouseCommittedStockValueResponse(
                    warehouse.WarehouseId.Value,
                    warehouse.Code,
                    warehouse.Name,
                    reservations.Sum(reservation => reservation.QuantityReserved),
                    reservations.Count,
                    reservations.Sum(reservation => reservation.QuantityReserved * reservation.UnitCostSnapshot),
                    details.Select(detail => detail.Response).ToList());
            })
            .ToList();
    }

    private sealed record ActiveReservationProjection(
        StockReservationId StockReservationId,
        PurchaseOrderLineId PurchaseOrderLineId,
        WarehouseId WarehouseId,
        decimal QuantityReserved,
        decimal UnitCostSnapshot);

    private sealed record PurchaseOrderLineProjection(
        PurchaseOrderLineId PurchaseOrderLineId,
        PurchaseOrderId PurchaseOrderId,
        InventoryItemId InventoryItemId,
        string Sku,
        string ItemName,
        InventoryTrackingMode TrackingMode);

    private sealed record PurchaseOrderProjection(
        PurchaseOrderId PurchaseOrderId,
        string PurchaseOrderNumber);

    private sealed record WarehouseProjection(
        WarehouseId WarehouseId,
        string Code,
        string Name);

    private sealed record ReservationDetailProjection(
        WarehouseId WarehouseId,
        string PurchaseOrderNumber,
        string Sku,
        WarehouseCommittedStockReservationResponse Response);
}
