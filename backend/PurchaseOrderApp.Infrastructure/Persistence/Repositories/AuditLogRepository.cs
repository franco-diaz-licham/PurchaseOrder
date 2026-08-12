using Microsoft.EntityFrameworkCore;
using PurchaseOrderApp.Application.Models;
using PurchaseOrderApp.Application.Ports;
using PurchaseOrderApp.Domain.Entities;
using PurchaseOrderApp.Domain.ValueObjects;

namespace PurchaseOrderApp.Infrastructure.Persistence.Repositories;

public sealed class AuditLogRepository(DatabaseContext db) : IAuditLogRepository
{
    public async Task<List<AuditLogResponse>> ListAsync(WarehouseId? warehouseId, CancellationToken cancellationToken)
    {
        var query = db.AuditLogEntries.AsNoTracking();

        if (warehouseId is not null)
        {
            query = query.Where(entry => entry.WarehouseId == warehouseId.Value);
        }

        var entries = await query
            .OrderByDescending(entry => entry.CreatedAt)
            .ToListAsync(cancellationToken);

        return entries
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
            .ToList();
    }

    public async Task AddAsync(AuditLogEntry auditLogEntry, CancellationToken cancellationToken)
    {
        await db.AuditLogEntries.AddAsync(auditLogEntry, cancellationToken);
    }
}
