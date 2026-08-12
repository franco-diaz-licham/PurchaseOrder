using PurchaseOrderApp.Application.Models;
using PurchaseOrderApp.Domain.ValueObjects;

namespace PurchaseOrderApp.Application.Ports;

/// <summary>
/// Reads immutable audit records for reservation and release actions.
/// </summary>
public interface IAuditLogRepository
{
    /// <summary>
    /// Lists audit entries, optionally filtered to one warehouse.
    /// </summary>
    Task<List<AuditLogResponse>> ListAsync(WarehouseId? warehouseId, CancellationToken cancellationToken);
}
