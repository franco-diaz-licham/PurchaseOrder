using Microsoft.EntityFrameworkCore;
using PurchaseOrderApp.Application.Models;
using PurchaseOrderApp.Application.Ports;
using PurchaseOrderApp.Domain.Entities;
using PurchaseOrderApp.Domain.Enums;
using PurchaseOrderApp.Domain.ValueObjects;

namespace PurchaseOrderApp.Infrastructure.Persistence.Repositories;

public sealed class FinanceRepository(DatabaseContext db) : IFinanceQueryRepository
{
    public async Task<List<WarehouseCommittedStockValueResponse>> ListWarehouseCommittedStockValuesAsync(CancellationToken cancellationToken)
    {
        var activeReservations = await db.StockReservations
            .AsNoTracking()
            .Where(reservation => reservation.Status == ReservationStatus.Active)
            .ToListAsync(cancellationToken);

        var lineIds = activeReservations
            .Select(reservation => reservation.PurchaseOrderLineId)
            .Distinct()
            .ToList();

        var lines = await db.PurchaseOrderLines
            .AsNoTracking()
            .Include(line => line.InventoryItem)
            .Where(line => lineIds.Contains(line.Id))
            .ToListAsync(cancellationToken);

        var lineById = lines.ToDictionary(line => line.Id);

        var purchaseOrderIds = lines
            .Select(line => line.PurchaseOrderId)
            .Distinct()
            .ToList();

        var purchaseOrders = await db.PurchaseOrders
            .AsNoTracking()
            .Where(order => purchaseOrderIds.Contains(order.Id))
            .ToListAsync(cancellationToken);

        var purchaseOrderById = purchaseOrders.ToDictionary(order => order.Id);

        var reservationDetails = activeReservations
            .Select(reservation => ToReservationResponse(reservation, lineById, purchaseOrderById))
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
            .ToListAsync(cancellationToken);

        return warehouses
            .Select(warehouse => {
                var reservations = reservationsByWarehouse.GetValueOrDefault(warehouse.Id, []);
                var details = reservationDetailsByWarehouse.GetValueOrDefault(warehouse.Id, []);
                return new WarehouseCommittedStockValueResponse(
                    warehouse.Id.Value,
                    warehouse.Code,
                    warehouse.Name,
                    reservations.Sum(reservation => reservation.QuantityReserved.Value),
                    reservations.Count,
                    reservations.Sum(reservation => reservation.QuantityReserved.Value * reservation.UnitCostSnapshot.Value),
                    details.Select(detail => detail.Response).ToList());
            })
            .ToList();
    }

    private static ReservationDetail? ToReservationResponse(
        StockReservation reservation,
        IReadOnlyDictionary<PurchaseOrderLineId, PurchaseOrderLine> lineById,
        IReadOnlyDictionary<PurchaseOrderId, PurchaseOrder> purchaseOrderById)
    {
        if (!lineById.TryGetValue(reservation.PurchaseOrderLineId, out var line)) return null;
        if (!purchaseOrderById.TryGetValue(line.PurchaseOrderId, out var purchaseOrder)) return null;

        var committedValue = reservation.QuantityReserved.Value * reservation.UnitCostSnapshot.Value;
        return new ReservationDetail(
            reservation.WarehouseId,
            purchaseOrder.PurchaseOrderNumber,
            line.InventoryItem.Sku,
            new WarehouseCommittedStockReservationResponse(
                reservation.Id.Value,
                purchaseOrder.Id.Value,
                purchaseOrder.PurchaseOrderNumber,
                line.Id.Value,
                line.InventoryItem.Id.Value,
                line.InventoryItem.Sku,
                line.InventoryItem.Name,
                reservation.QuantityReserved.Value,
                reservation.UnitCostSnapshot.Value,
                committedValue));
    }

    private sealed record ReservationDetail(
        WarehouseId WarehouseId,
        string PurchaseOrderNumber,
        string Sku,
        WarehouseCommittedStockReservationResponse Response);
}
