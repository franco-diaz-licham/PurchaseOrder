using PurchaseOrderApp.BL.Enums;

namespace PurchaseOrderApp.BL.Models;

public sealed class PurchaseOrder
{
    public Guid Id { get; set; }

    public string PurchaseOrderNumber { get; set; } = string.Empty;

    public Guid WarehouseId { get; set; }

    public PurchaseOrderStatus Status { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public List<PurchaseOrderLine> Lines { get; set; } = [];
}
