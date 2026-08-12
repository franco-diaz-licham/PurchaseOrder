using Microsoft.AspNetCore.Mvc;
using PurchaseOrderApp.Api.Controllers.Models;
using PurchaseOrderApp.Api.Helpers;
using PurchaseOrderApp.Api.Models;
using PurchaseOrderApp.Application.Models;
using PurchaseOrderApp.Application.UseCases;
using PurchaseOrderApp.Domain.Enums;
using PurchaseOrderApp.Domain.ValueObjects;

namespace PurchaseOrderApp.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class ReservationController(IReservationService reservationService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ReservationResponse>>>> GetAll(
        [FromQuery] Guid? warehouseId,
        [FromQuery] ReservationStatus? status,
        CancellationToken cancellationToken)
    {
        WarehouseId? parsedWarehouseId = warehouseId.HasValue ? new WarehouseId(warehouseId.Value) : null;
        var result = await reservationService.ListAsync(parsedWarehouseId, status, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ReservationResponse>>> Create([FromBody] CreateReservationRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateReservationCommand(
            new PurchaseOrderLineId(request.PurchaseOrderLineId),
            new WarehouseId(request.WarehouseId),
            new Quantity(request.Quantity),
            request.User,
            DateTimeOffset.UtcNow);

        var result = await reservationService.ReserveAsync(command, cancellationToken);
        var locationUrl = result.Value is null ? null : $"/api/reservation/{result.Value.StockReservationId}";
        return result.ToActionResult(locationUrl);
    }

    [HttpPost("{stockReservationId:guid}/release")]
    public async Task<ActionResult<ApiResponse<ReservationResponse>>> Release(
        Guid stockReservationId,
        [FromBody] ReleaseReservationRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ReleaseReservationCommand(
            new StockReservationId(stockReservationId),
            new Quantity(request.Quantity),
            request.User,
            DateTimeOffset.UtcNow);

        var result = await reservationService.ReleaseAsync(command, cancellationToken);
        return result.ToActionResult();
    }
}
