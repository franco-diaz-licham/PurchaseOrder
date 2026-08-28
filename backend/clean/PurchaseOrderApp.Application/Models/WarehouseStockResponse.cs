namespace PurchaseOrderApp.Application.Models;

public sealed record WarehouseStockResponse(
    Guid WarehouseId,
    Guid InventoryItemId,
    decimal OnHandQuantity,
    decimal ActiveReservedQuantity,
    decimal AvailableQuantity);
