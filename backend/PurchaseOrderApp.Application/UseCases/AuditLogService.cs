using PurchaseOrderApp.Application.Models;
using PurchaseOrderApp.Application.Ports;

namespace PurchaseOrderApp.Application.UseCases;

/// <summary>
/// Coordinates audit log read operations.
/// </summary>
public interface IAuditLogService
{
    /// <summary>
    /// Lists all audit log entries.
    /// </summary>
    Task<Result<List<AuditLogResponse>>> ListAsync(CancellationToken cancellationToken);
}

public sealed class AuditLogService(IAuditLogRepository auditLogRepository) : IAuditLogService
{
    public async Task<Result<List<AuditLogResponse>>> ListAsync(CancellationToken cancellationToken)
    {
        var entries = await auditLogRepository.ListAsync(cancellationToken);
        return Result.Success(entries);
    }
}
