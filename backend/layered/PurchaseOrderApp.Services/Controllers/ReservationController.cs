using Microsoft.AspNetCore.Mvc;
using PurchaseOrderApp.BL.Commands;
using PurchaseOrderApp.BL.Responses;
using PurchaseOrderApp.BL.Workflows;
using PurchaseOrderApp.Services.Helpers;
using PurchaseOrderApp.Services.Models;

namespace PurchaseOrderApp.Services.Controllers;

[ApiController]
[Route("api/reservation")]
public sealed class ReservationController(IReservationService reservationService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ReservationResponse>>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await reservationService.ListAsync(cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ReservationResponse>>> Create([FromBody] CreateReservationRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateReservationCommand(request.PurchaseOrderLineId, request.WarehouseId, request.Quantity, request.User, DateTimeOffset.UtcNow);
        var result = await reservationService.ReserveAsync(command, cancellationToken);
        return result.ToActionResult(result.Value is null ? null : $"/api/reservation/{result.Value.StockReservationId}");
    }

    [HttpPost("{stockReservationId:guid}/release")]
    public async Task<ActionResult<ApiResponse<ReservationResponse>>> Release(Guid stockReservationId, [FromBody] ReleaseReservationRequest request, CancellationToken cancellationToken)
    {
        var command = new ReleaseReservationCommand(stockReservationId, request.Quantity, request.User, DateTimeOffset.UtcNow);
        var result = await reservationService.ReleaseAsync(command, cancellationToken);
        return result.ToActionResult();
    }
}
