namespace PurchaseOrderApp.Domain.ValueObjects;

/// <summary>
/// Strongly typed identifier for an audit log entry.
/// </summary>
public readonly record struct AuditLogEntryId(Guid Value);
