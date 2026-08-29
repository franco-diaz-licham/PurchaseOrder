namespace PurchaseOrderApp.BL.Models;

public sealed class WarehouseStock : EntityMetadata
{
    public Guid Id { get; set; }

    public Guid WarehouseId { get; set; }

    public Guid InventoryItemId { get; set; }

    public decimal OnHandQuantity { get; set; }
}
