namespace PurchaseOrderApp.BL.Models;

public sealed class Warehouse : EntityMetadata
{
    public Guid Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
}
