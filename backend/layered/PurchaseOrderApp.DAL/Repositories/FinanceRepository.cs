using Microsoft.EntityFrameworkCore;
using PurchaseOrderApp.BL.Enums;
using PurchaseOrderApp.BL.Ports;
using PurchaseOrderApp.BL.Responses;

namespace PurchaseOrderApp.DAL.Repositories;

public sealed class FinanceRepository(PurchaseOrderDbContext db) : IFinanceQueryRepository
{
    public async Task<List<WarehouseCommittedStockValueResponse>> ListWarehouseCommittedStockValuesAsync(CancellationToken cancellationToken)
    {
        var activeReservations = await db.StockReservations
            .AsNoTracking()
            .Where(reservation => reservation.Status == ReservationStatus.Active)
            .Select(reservation => new {
                reservation.Id,
                reservation.PurchaseOrderLineId,
                reservation.WarehouseId,
                reservation.QuantityReserved,
                reservation.UnitCostSnapshot
            })
            .ToListAsync(cancellationToken);

        var lineIds = activeReservations.Select(reservation => reservation.PurchaseOrderLineId).Distinct().ToList();
        var lines = await db.PurchaseOrderLines
            .AsNoTracking()
            .Where(line => lineIds.Contains(line.Id))
            .Select(line => new {
                line.Id,
                line.PurchaseOrderId,
                line.InventoryItemId,
                Sku = line.InventoryItem!.Sku,
                ItemName = line.InventoryItem.Name,
                TrackingMode = line.InventoryItem.TrackingMode
            })
            .ToListAsync(cancellationToken);

        var lineById = lines.ToDictionary(line => line.Id);
        var purchaseOrderIds = lines.Select(line => line.PurchaseOrderId).Distinct().ToList();
        var purchaseOrders = await db.PurchaseOrders
            .AsNoTracking()
            .Where(order => purchaseOrderIds.Contains(order.Id))
            .Select(order => new { order.Id, order.PurchaseOrderNumber })
            .ToListAsync(cancellationToken);

        var purchaseOrderById = purchaseOrders.ToDictionary(order => order.Id);
        var details = activeReservations
            .Select(reservation => {
                if (!lineById.TryGetValue(reservation.PurchaseOrderLineId, out var line)) return null;
                if (!purchaseOrderById.TryGetValue(line.PurchaseOrderId, out var purchaseOrder)) return null;

                var committedValue = reservation.QuantityReserved * reservation.UnitCostSnapshot;
                return new {
                    reservation.WarehouseId,
                    purchaseOrder.PurchaseOrderNumber,
                    line.Sku,
                    Response = new WarehouseCommittedStockReservationResponse(
                        reservation.Id,
                        purchaseOrder.Id,
                        purchaseOrder.PurchaseOrderNumber,
                        line.Id,
                        line.InventoryItemId,
                        line.Sku,
                        line.ItemName,
                        line.TrackingMode.ToString(),
                        reservation.QuantityReserved,
                        reservation.UnitCostSnapshot,
                        committedValue)
                };
            })
            .Where(detail => detail is not null)
            .Select(detail => detail!)
            .ToList();

        var reservationsByWarehouse = activeReservations
            .GroupBy(reservation => reservation.WarehouseId)
            .ToDictionary(group => group.Key, group => group.ToList());

        var detailsByWarehouse = details
            .GroupBy(detail => detail.WarehouseId)
            .ToDictionary(group => group.Key, group => group.OrderBy(detail => detail.PurchaseOrderNumber).ThenBy(detail => detail.Sku).ToList());

        var warehouses = await db.Warehouses
            .AsNoTracking()
            .OrderBy(warehouse => warehouse.Code)
            .Select(warehouse => new { warehouse.Id, warehouse.Code, warehouse.Name })
            .ToListAsync(cancellationToken);

        return warehouses.Select(warehouse => {
            var reservations = reservationsByWarehouse.GetValueOrDefault(warehouse.Id, []);
            var warehouseDetails = detailsByWarehouse.GetValueOrDefault(warehouse.Id, []);
            return new WarehouseCommittedStockValueResponse(
                warehouse.Id,
                warehouse.Code,
                warehouse.Name,
                reservations.Sum(reservation => reservation.QuantityReserved),
                reservations.Count,
                reservations.Sum(reservation => reservation.QuantityReserved * reservation.UnitCostSnapshot),
                warehouseDetails.Select(detail => detail.Response).ToList());
        }).ToList();
    }
}
