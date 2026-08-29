namespace PurchaseOrderApp.BL.Responses;

public sealed record WarehouseCommittedStockValueResponse(
    Guid WarehouseId,
    string WarehouseCode,
    string WarehouseName,
    decimal ReservedQuantity,
    int ReservationCount,
    decimal CommittedValue,
    List<WarehouseCommittedStockReservationResponse> Reservations);

public sealed record WarehouseCommittedStockReservationResponse(
    Guid StockReservationId,
    Guid PurchaseOrderId,
    string PurchaseOrderNumber,
    Guid PurchaseOrderLineId,
    Guid InventoryItemId,
    string Sku,
    string ItemName,
    string TrackingMode,
    decimal QuantityReserved,
    decimal UnitCostSnapshot,
    decimal CommittedValue);
