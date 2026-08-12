using Microsoft.EntityFrameworkCore;
using PurchaseOrderApp.Application.Models;
using PurchaseOrderApp.Application.Ports;

namespace PurchaseOrderApp.Infrastructure.Repositories;

public sealed class AuditLogRepository(DatabaseContext db) : IAuditLogRepository
{
    public Task<List<AuditLogResponse>> ListAsync(CancellationToken cancellationToken)
    {
        return db.AuditLogEntries
            .AsNoTracking()
            .OrderByDescending(entry => entry.CreatedAt)
            .Select(entry => new AuditLogResponse(
                entry.Id.Value,
                entry.Action.ToString(),
                entry.InventoryItemId.Value,
                entry.WarehouseId.Value,
                entry.PurchaseOrderLineId.Value,
                entry.StockReservationId.Value,
                entry.Quantity.Value,
                entry.ResultingAvailableQuantity.Value,
                entry.CreatedBy,
                entry.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
