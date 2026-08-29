namespace PurchaseOrderApp.BL.Responses;

public sealed record InventoryItemResponse(
    Guid InventoryItemId,
    string Sku,
    string Name,
    string Category,
    string TrackingMode,
    decimal StandardCost);

public sealed record WarehouseResponse(
    Guid WarehouseId,
    string Code,
    string Name);

public sealed record WarehouseStockResponse(
    Guid WarehouseId,
    Guid InventoryItemId,
    decimal OnHandQuantity,
    decimal ActiveReservedQuantity,
    decimal AvailableQuantity);
