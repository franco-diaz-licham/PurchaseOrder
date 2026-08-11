using Microsoft.EntityFrameworkCore;
using PurchaseOrder.Application.Models;
using PurchaseOrder.Application.Ports;
using PurchaseOrder.Domain.Enums;

namespace PurchaseOrder.Infrastructure.Persistence.Repositories;

public sealed class FinanceRepository(DatabaseContext db) : IFinanceQueryRepository
{
    public async Task<List<WarehouseCommittedStockValueResponse>> ListWarehouseCommittedStockValuesAsync(CancellationToken cancellationToken)
    {
        var committedValues = db.StockReservations
            .AsNoTracking()
            .Where(reservation => reservation.Status == ReservationStatus.Active)
            .GroupBy(reservation => reservation.WarehouseId)
            .Select(group => new {
                WarehouseId = group.Key,
                Value = group.Sum(reservation => reservation.QuantityReserved.Value * reservation.UnitCostSnapshot.Value)
            });

        var warehouseSummaries = db.Warehouses
            .AsNoTracking()
            .Select(warehouse => new WarehouseCommittedStockValueResponse(
                warehouse.Id.Value,
                warehouse.Code,
                warehouse.Name,
                committedValues
                    .Where(value => value.WarehouseId == warehouse.Id)
                    .Select(value => value.Value)
                    .FirstOrDefault()))
            .OrderBy(summary => summary.WarehouseCode);

        return await warehouseSummaries.ToListAsync(cancellationToken);
    }
}
