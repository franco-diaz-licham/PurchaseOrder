using PurchaseOrderApp.BL.Common;
using PurchaseOrderApp.BL.Ports;
using PurchaseOrderApp.BL.Responses;

namespace PurchaseOrderApp.BL.Queries.AuditLogs;

public sealed record ListAuditLogEntriesQuery;

public sealed class ListAuditLogEntriesQueryHandler(IAuditLogRepository auditLogRepository)
{
    public async Task<Result<List<AuditLogResponse>>> ExecuteAsync(ListAuditLogEntriesQuery query, CancellationToken cancellationToken)
    {
        return Result.Success(await auditLogRepository.ListResponsesAsync(cancellationToken));
    }
}
