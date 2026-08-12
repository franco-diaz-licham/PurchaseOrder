using Microsoft.EntityFrameworkCore;
using PurchaseOrderApp.Application.Models;
using PurchaseOrderApp.Application.Ports;
using PurchaseOrderApp.Domain.Enums;

namespace PurchaseOrderApp.Infrastructure.Persistence.Repositories;

public sealed class FinanceRepository(DatabaseContext db) : IFinanceQueryRepository
{
    public async Task<List<WarehouseCommittedStockValueResponse>> ListWarehouseCommittedStockValuesAsync(CancellationToken cancellationToken)
    {
        var activeReservations = await db.StockReservations
            .AsNoTracking()
            .Where(reservation => reservation.Status == ReservationStatus.Active)
            .ToListAsync(cancellationToken);

        var committedValues = activeReservations
            .GroupBy(reservation => reservation.WarehouseId)
            .ToDictionary(
                group => group.Key,
                group => group.Sum(reservation => reservation.QuantityReserved.Value * reservation.UnitCostSnapshot.Value));

        var warehouses = await db.Warehouses
            .AsNoTracking()
            .OrderBy(warehouse => warehouse.Code)
            .ToListAsync(cancellationToken);

        return warehouses
            .Select(warehouse => new WarehouseCommittedStockValueResponse(
                warehouse.Id.Value,
                warehouse.Code,
                warehouse.Name,
                committedValues.GetValueOrDefault(warehouse.Id)))
            .ToList();
    }
}
