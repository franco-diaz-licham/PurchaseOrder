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
    [HttpGet("summary")]
    public async Task<ActionResult<ApiResponse<List<PurchaseOrderSummaryResponse>>>> GetSummary(CancellationToken cancellationToken)
    {
        var result = await purchaseOrderService.ListSummariesAsync(cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("{purchaseOrderId:guid}")]
    public async Task<ActionResult<ApiResponse<PurchaseOrderResponse>>> Get(Guid purchaseOrderId, CancellationToken cancellationToken)
    {
        var result = await purchaseOrderService.GetAsync(new PurchaseOrderId(purchaseOrderId), cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PurchaseOrderResponse>>> Submit([FromBody] SubmitPurchaseOrderRequest request, CancellationToken cancellationToken)
    {
        var lines = new List<SubmitPurchaseOrderLineCommand>();
        if (request.Lines is not null) {
            foreach (var requestLine in request.Lines) {
                var inventoryItemId = new InventoryItemId(requestLine.InventoryItemId);
                var quantityOrdered = new Quantity(requestLine.QuantityOrdered);
                var line = new SubmitPurchaseOrderLineCommand(inventoryItemId, quantityOrdered);
                lines.Add(line);
            }
        }

        var command = new SubmitPurchaseOrderCommand(
            new WarehouseId(request.WarehouseId),
            lines,
            request.User,
            DateTimeOffset.UtcNow);

        var result = await purchaseOrderService.SubmitAsync(command, cancellationToken);
        return result.ToActionResult($"/api/purchase-order/{result.Value?.PurchaseOrderId}");
    }

    [HttpPost("{purchaseOrderId:guid}/lines")]
    public async Task<ActionResult<ApiResponse<PurchaseOrderResponse>>> AddLine(Guid purchaseOrderId, [FromBody] AddPurchaseOrderLineRequest request, CancellationToken cancellationToken)
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

    [HttpDelete("{purchaseOrderId:guid}/lines/{purchaseOrderLineId:guid}")]
    public async Task<ActionResult<ApiResponse<PurchaseOrderResponse>>> RemoveLine(Guid purchaseOrderId, Guid purchaseOrderLineId, [FromBody] RemovePurchaseOrderLineRequest request, CancellationToken cancellationToken)
    {
        var command = new RemovePurchaseOrderLineCommand(
            new PurchaseOrderId(purchaseOrderId),
            new PurchaseOrderLineId(purchaseOrderLineId),
            request.User,
            DateTimeOffset.UtcNow);

        var result = await purchaseOrderService.RemoveLineAsync(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPut("{purchaseOrderId:guid}/lines/{purchaseOrderLineId:guid}")]
    public async Task<ActionResult<ApiResponse<PurchaseOrderResponse>>> UpdateLine(Guid purchaseOrderId, Guid purchaseOrderLineId, [FromBody] UpdatePurchaseOrderLineRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdatePurchaseOrderLineCommand(
            new PurchaseOrderId(purchaseOrderId),
            new PurchaseOrderLineId(purchaseOrderLineId),
            new Quantity(request.QuantityOrdered),
            request.User,
            DateTimeOffset.UtcNow);

        var result = await purchaseOrderService.UpdateLineAsync(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPut("{purchaseOrderId:guid}/approve")]
    public async Task<ActionResult<ApiResponse<PurchaseOrderResponse>>> Approve(Guid purchaseOrderId, [FromBody] ChangePurchaseOrderStatusRequest request, CancellationToken cancellationToken)
    {
        var command = new ChangePurchaseOrderStatusCommand(new PurchaseOrderId(purchaseOrderId), request.User, DateTimeOffset.UtcNow);
        var result = await purchaseOrderService.ApproveAsync(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPut("{purchaseOrderId:guid}/close")]
    public async Task<ActionResult<ApiResponse<PurchaseOrderResponse>>> Close(Guid purchaseOrderId, [FromBody] ChangePurchaseOrderStatusRequest request, CancellationToken cancellationToken)
    {
        var command = new ChangePurchaseOrderStatusCommand(new PurchaseOrderId(purchaseOrderId), request.User, DateTimeOffset.UtcNow);
        var result = await purchaseOrderService.CloseAsync(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPut("{purchaseOrderId:guid}/cancel")]
    public async Task<ActionResult<ApiResponse<PurchaseOrderResponse>>> Cancel(Guid purchaseOrderId, [FromBody] ChangePurchaseOrderStatusRequest request, CancellationToken cancellationToken)
    {
        var command = new ChangePurchaseOrderStatusCommand(new PurchaseOrderId(purchaseOrderId), request.User, DateTimeOffset.UtcNow);
        var result = await purchaseOrderService.CancelAsync(command, cancellationToken);
        return result.ToActionResult();
    }

}
