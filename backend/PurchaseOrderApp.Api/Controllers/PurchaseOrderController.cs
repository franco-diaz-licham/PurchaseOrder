using Microsoft.AspNetCore.Mvc;
using PurchaseOrderApp.Api.Controllers.Models;
using PurchaseOrderApp.Api.Helpers;
using PurchaseOrderApp.Api.Models;
using PurchaseOrderApp.Application.Models;
using PurchaseOrderApp.Application.UseCases;
using PurchaseOrderApp.Domain.ValueObjects;

namespace PurchaseOrderApp.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class PurchaseOrderController(IPurchaseOrderService purchaseOrderService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<PurchaseOrderResponse>>>> GetAll(
        [FromQuery] Guid? warehouseId,
        CancellationToken cancellationToken)
    {
        WarehouseId? parsedWarehouseId = warehouseId.HasValue ? new WarehouseId(warehouseId.Value) : null;
        var result = await purchaseOrderService.ListAsync(parsedWarehouseId, cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("{purchaseOrderId:guid}")]
    public async Task<ActionResult<ApiResponse<PurchaseOrderResponse>>> Get(
        Guid purchaseOrderId,
        CancellationToken cancellationToken)
    {
        var result = await purchaseOrderService.GetAsync(new PurchaseOrderId(purchaseOrderId), cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PurchaseOrderResponse>>> Submit(
        [FromBody] SubmitPurchaseOrderRequest request,
        CancellationToken cancellationToken)
    {
        var command = new SubmitPurchaseOrderCommand(
            request.PurchaseOrderNumber,
            new WarehouseId(request.WarehouseId),
            request.Lines?
                .Select(line => new SubmitPurchaseOrderLineCommand(
                    new InventoryItemId(line.InventoryItemId),
                    new Quantity(line.QuantityOrdered)))
                .ToList() ?? [],
            request.User,
            DateTimeOffset.UtcNow);

        var result = await purchaseOrderService.SubmitAsync(command, cancellationToken);
        return result.ToActionResult($"/api/PurchaseOrder/{result.Value?.PurchaseOrderId}");
    }

    [HttpPost("{purchaseOrderId:guid}/lines")]
    public async Task<ActionResult<ApiResponse<PurchaseOrderResponse>>> AddLine(
        Guid purchaseOrderId,
        [FromBody] AddPurchaseOrderLineRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddPurchaseOrderLineCommand(
            new PurchaseOrderId(purchaseOrderId),
            new InventoryItemId(request.InventoryItemId),
            new Quantity(request.QuantityOrdered),
            request.User,
            DateTimeOffset.UtcNow);

        var result = await purchaseOrderService.AddLineAsync(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPut("{purchaseOrderId:guid}/approve")]
    public async Task<ActionResult<ApiResponse<PurchaseOrderResponse>>> Approve(
        Guid purchaseOrderId,
        [FromBody] ChangePurchaseOrderStatusRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ChangePurchaseOrderStatusCommand(new PurchaseOrderId(purchaseOrderId), request.User, DateTimeOffset.UtcNow);
        var result = await purchaseOrderService.ApproveAsync(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPut("{purchaseOrderId:guid}/close")]
    public async Task<ActionResult<ApiResponse<PurchaseOrderResponse>>> Close(
        Guid purchaseOrderId,
        [FromBody] ChangePurchaseOrderStatusRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ChangePurchaseOrderStatusCommand(new PurchaseOrderId(purchaseOrderId), request.User, DateTimeOffset.UtcNow);
        var result = await purchaseOrderService.CloseAsync(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPut("{purchaseOrderId:guid}/cancel")]
    public async Task<ActionResult<ApiResponse<PurchaseOrderResponse>>> Cancel(
        Guid purchaseOrderId,
        [FromBody] ChangePurchaseOrderStatusRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ChangePurchaseOrderStatusCommand(new PurchaseOrderId(purchaseOrderId), request.User, DateTimeOffset.UtcNow);
        var result = await purchaseOrderService.CancelAsync(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("approved-lines")]
    public async Task<ActionResult<ApiResponse<List<ApprovedPurchaseOrderLineResponse>>>> GetApprovedLines(
        [FromQuery] Guid warehouseId,
        CancellationToken cancellationToken)
    {
        var result = await purchaseOrderService.ListApprovedOutstandingLinesAsync(new WarehouseId(warehouseId), cancellationToken);
        return result.ToActionResult();
    }
}
