using PurchaseOrderApp.Domain.ValueObjects;

namespace PurchaseOrderApp.Application.Models;

public sealed record CreateReservationCommand(
    PurchaseOrderLineId PurchaseOrderLineId,
    WarehouseId WarehouseId,
    Quantity Quantity,
    string User,
    DateTimeOffset OccurredAt);

public sealed record ReleaseReservationCommand(
    StockReservationId StockReservationId,
    Quantity Quantity,
    string User,
    DateTimeOffset OccurredAt);
