namespace PurchaseOrderApp.BL.Models;

public sealed class PurchaseOrderLine : EntityMetadata
{
    public Guid Id { get; set; }

    public Guid PurchaseOrderId { get; set; }

    public Guid InventoryItemId { get; set; }

    public decimal QuantityOrdered { get; set; }

    public decimal QuantityReserved { get; set; }

    public InventoryItem? InventoryItem { get; set; }

    public decimal QuantityRemaining => QuantityOrdered - QuantityReserved;

    public bool HasOutstandingQuantity => QuantityReserved < QuantityOrdered;
}
