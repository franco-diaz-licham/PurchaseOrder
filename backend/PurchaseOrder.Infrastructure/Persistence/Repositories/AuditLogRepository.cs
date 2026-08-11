using PurchaseOrder.Application.Ports;
using PurchaseOrder.Domain.Entities;

namespace PurchaseOrder.Infrastructure.Persistence.Repositories;

public sealed class AuditLogRepository(DatabaseContext db) : IAuditLogRepository
{
    public async Task AddAsync(AuditLogEntry auditLogEntry, CancellationToken cancellationToken)
    {
        await db.AuditLogEntries.AddAsync(auditLogEntry, cancellationToken);
    }
}
