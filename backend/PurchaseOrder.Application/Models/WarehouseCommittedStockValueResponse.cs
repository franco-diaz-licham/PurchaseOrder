namespace PurchaseOrder.Application.Models;

public sealed record WarehouseCommittedStockValueResponse(
    Guid WarehouseId,
    string WarehouseCode,
    string WarehouseName,
    decimal CommittedValue);
