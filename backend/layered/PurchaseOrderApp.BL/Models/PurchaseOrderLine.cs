namespace PurchaseOrderApp.BL.Models;

public sealed class PurchaseOrderLine
{
    public Guid Id { get; set; }

    public Guid PurchaseOrderId { get; set; }

    public Guid InventoryItemId { get; set; }

    public decimal QuantityOrdered { get; set; }

    public decimal QuantityReserved { get; set; }
}
