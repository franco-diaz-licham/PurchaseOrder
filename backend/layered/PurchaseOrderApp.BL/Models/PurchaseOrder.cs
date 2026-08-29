using PurchaseOrderApp.BL.Enums;

namespace PurchaseOrderApp.BL.Models;

public sealed class PurchaseOrder : EntityMetadata
{
    public Guid Id { get; set; }

    public string PurchaseOrderNumber { get; set; } = string.Empty;

    public Guid WarehouseId { get; set; }

    public PurchaseOrderStatus Status { get; set; }

    public Warehouse? Warehouse { get; set; }

    public List<PurchaseOrderLine> Lines { get; set; } = [];
}
