using PurchaseOrderApp.Application.Models;
using PurchaseOrderApp.Application.Ports;
using PurchaseOrderApp.Domain.ValueObjects;

namespace PurchaseOrderApp.Application.UseCases;

/// <summary>
/// Coordinates audit log read operations.
/// </summary>
public interface IAuditLogService
{
    /// <summary>
    /// Lists audit log entries, optionally filtered by warehouse.
    /// </summary>
    Task<Result<List<AuditLogResponse>>> ListAsync(WarehouseId? warehouseId, CancellationToken cancellationToken);
}

public sealed class AuditLogService(IAuditLogRepository auditLogRepository) : IAuditLogService
{
    public async Task<Result<List<AuditLogResponse>>> ListAsync(WarehouseId? warehouseId, CancellationToken cancellationToken)
    {
        if (warehouseId is not null && warehouseId.Value.Value == Guid.Empty) return Result.Fail<List<AuditLogResponse>>("Warehouse id is required.", ResultStatus.Invalid);
        var entries = await auditLogRepository.ListAsync(warehouseId, cancellationToken);
        return Result.Success(entries);
    }
}
