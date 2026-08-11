namespace PurchaseOrderApp.Application.Models;

public sealed record WarehouseResponse(
    Guid WarehouseId,
    string Code,
    string Name);
