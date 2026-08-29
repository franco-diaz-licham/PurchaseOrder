using PurchaseOrderApp.BL.Enums;

namespace PurchaseOrderApp.BL.Models;

public sealed class AuditLogEntry : EntityMetadata
{
    public Guid Id { get; set; }

    public AuditAction Action { get; set; }

    public Guid InventoryItemId { get; set; }

    public Guid WarehouseId { get; set; }

    public Guid PurchaseOrderLineId { get; set; }

    public Guid StockReservationId { get; set; }

    public decimal Quantity { get; set; }

    public decimal ResultingAvailableQuantity { get; set; }
}
