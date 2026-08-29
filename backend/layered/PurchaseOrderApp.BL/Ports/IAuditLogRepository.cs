using PurchaseOrderApp.BL.Models;
using PurchaseOrderApp.BL.Responses;

namespace PurchaseOrderApp.BL.Ports;

public interface IAuditLogRepository
{
    Task<List<AuditLogResponse>> ListResponsesAsync(CancellationToken cancellationToken);

    Task AddAsync(AuditLogEntry auditLogEntry, CancellationToken cancellationToken);
}
