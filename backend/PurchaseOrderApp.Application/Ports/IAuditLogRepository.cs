using PurchaseOrderApp.Domain.Entities;

namespace PurchaseOrderApp.Application.Ports;

/// <summary>
/// Persists immutable audit records for reservation and release actions.
/// </summary>
public interface IAuditLogRepository
{
    /// <summary>
    /// Adds an audit log entry to the current unit of work.
    /// </summary>
    Task AddAsync(AuditLogEntry auditLogEntry, CancellationToken cancellationToken);
}
