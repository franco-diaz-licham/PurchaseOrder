using Microsoft.EntityFrameworkCore;
using PurchaseOrderApp.BL.Models;
using PurchaseOrderApp.BL.Ports;
using PurchaseOrderApp.BL.Responses;

namespace PurchaseOrderApp.DAL.Repositories;

public sealed class AuditLogRepository(PurchaseOrderDbContext db) : IAuditLogRepository
{
    public Task<List<AuditLogResponse>> ListResponsesAsync(CancellationToken cancellationToken)
    {
        return db.AuditLogEntries
            .AsNoTracking()
            .OrderByDescending(entry => entry.CreatedAt)
            .Select(entry => new AuditLogResponse(
                entry.Id,
                entry.Action.ToString(),
                entry.InventoryItemId,
                entry.WarehouseId,
                entry.PurchaseOrderLineId,
                entry.StockReservationId,
                entry.Quantity,
                entry.ResultingAvailableQuantity,
                entry.CreatedBy,
                entry.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(AuditLogEntry auditLogEntry, CancellationToken cancellationToken)
    {
        await db.AuditLogEntries.AddAsync(auditLogEntry, cancellationToken);
    }
}
