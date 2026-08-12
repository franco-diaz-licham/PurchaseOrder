using PurchaseOrderApp.Application.Models;

namespace PurchaseOrderApp.Application.Ports;

/// <summary>
/// Reads immutable audit records for reservation and release actions.
/// </summary>
public interface IAuditLogRepository
{
    /// <summary>
    /// Lists all audit entries.
    /// </summary>
    Task<List<AuditLogResponse>> ListAsync(CancellationToken cancellationToken);
}
