using PurchaseOrderApp.Domain.Entities;
using PurchaseOrderApp.Application.Models;
using PurchaseOrderApp.Domain.ValueObjects;

namespace PurchaseOrderApp.Application.Ports;

/// <summary>
/// Persists immutable audit records for reservation and release actions.
/// </summary>
public interface IAuditLogRepository
{
    /// <summary>
    /// Lists audit entries, optionally filtered to one warehouse.
    /// </summary>
    Task<List<AuditLogResponse>> ListAsync(WarehouseId? warehouseId, CancellationToken cancellationToken);

    /// <summary>
    /// Adds an audit log entry to the current unit of work.
    /// </summary>
    Task AddAsync(AuditLogEntry auditLogEntry, CancellationToken cancellationToken);
}
