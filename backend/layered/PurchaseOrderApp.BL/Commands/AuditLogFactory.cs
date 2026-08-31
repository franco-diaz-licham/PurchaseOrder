using PurchaseOrderApp.BL.Enums;
using PurchaseOrderApp.BL.Models;

namespace PurchaseOrderApp.BL.Commands;

internal static class AuditLogFactory
{
    public static AuditLogEntry Create(
        AuditAction action,
        StockReservation reservation,
        decimal quantity,
        decimal resultingAvailableQuantity,
        string user,
        DateTimeOffset occurredAt)
    {
        return new AuditLogEntry {
            Id = Guid.NewGuid(),
            Action = action,
            InventoryItemId = reservation.InventoryItemId,
            WarehouseId = reservation.WarehouseId,
            PurchaseOrderLineId = reservation.PurchaseOrderLineId,
            StockReservationId = reservation.Id,
            Quantity = quantity,
            ResultingAvailableQuantity = resultingAvailableQuantity,
            CreatedBy = user.Trim(),
            CreatedAt = occurredAt
        };
    }
}
