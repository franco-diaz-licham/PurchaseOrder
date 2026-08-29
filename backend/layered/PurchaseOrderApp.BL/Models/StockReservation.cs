using PurchaseOrderApp.BL.Enums;

namespace PurchaseOrderApp.BL.Models;

public sealed class StockReservation : EntityMetadata
{
    public Guid Id { get; set; }

    public Guid PurchaseOrderLineId { get; set; }

    public Guid WarehouseId { get; set; }

    public Guid InventoryItemId { get; set; }

    public decimal QuantityReserved { get; set; }

    public decimal UnitCostSnapshot { get; set; }

    public ReservationStatus Status { get; set; }

    public decimal CommittedValue => UnitCostSnapshot * QuantityReserved;
}
