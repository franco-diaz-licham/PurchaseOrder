namespace PurchaseOrderApp.Application.Models;

public sealed record ReservationResponse(
    Guid StockReservationId,
    Guid PurchaseOrderLineId,
    Guid WarehouseId,
    Guid InventoryItemId,
    decimal QuantityReserved,
    decimal UnitCostSnapshot,
    string Status);
