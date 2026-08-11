namespace PurchaseOrderApp.Api.Controllers.Models;

public sealed record CreateReservationRequest(
    Guid PurchaseOrderLineId,
    Guid WarehouseId,
    decimal Quantity,
    string User);

public sealed record ReleaseReservationRequest(
    decimal Quantity,
    string User);
