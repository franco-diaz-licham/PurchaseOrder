using PurchaseOrderApp.Application.Ports;
using PurchaseOrderApp.Domain.Entities;

namespace PurchaseOrderApp.Infrastructure.Persistence.Repositories;

public sealed class AuditLogRepository(DatabaseContext db) : IAuditLogRepository
{
    public async Task AddAsync(AuditLogEntry auditLogEntry, CancellationToken cancellationToken)
    {
        await db.AuditLogEntries.AddAsync(auditLogEntry, cancellationToken);
    }
}
