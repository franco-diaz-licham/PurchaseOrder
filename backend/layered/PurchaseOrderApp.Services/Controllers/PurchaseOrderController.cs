using Microsoft.AspNetCore.Mvc;
using PurchaseOrderApp.BL.Commands;
using PurchaseOrderApp.BL.Responses;
using PurchaseOrderApp.BL.Workflows;
using PurchaseOrderApp.Services.Helpers;
using PurchaseOrderApp.Services.Models;

namespace PurchaseOrderApp.Services.Controllers;

[ApiController]
[Route("api/purchase-order")]
public sealed class PurchaseOrderController(IPurchaseOrderWorkflowService purchaseOrderService) : ControllerBase
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
        var result = await purchaseOrderService.GetAsync(purchaseOrderId, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PurchaseOrderResponse>>> Submit([FromBody] SubmitPurchaseOrderRequest request, CancellationToken cancellationToken)
    {
        var lines = request.Lines?.Select(line => new SubmitPurchaseOrderLineCommand(line.InventoryItemId, line.QuantityOrdered)).ToList() ?? [];
        var command = new SubmitPurchaseOrderCommand(request.WarehouseId, lines, request.User, DateTimeOffset.UtcNow);
        var result = await purchaseOrderService.SubmitAsync(command, cancellationToken);
        return result.ToActionResult($"/api/purchase-order/{result.Value?.PurchaseOrderId}");
    }

    [HttpPost("{purchaseOrderId:guid}/lines")]
    public async Task<ActionResult<ApiResponse<PurchaseOrderResponse>>> AddLine(Guid purchaseOrderId, [FromBody] AddPurchaseOrderLineRequest request, CancellationToken cancellationToken)
    {
        var command = new AddPurchaseOrderLineCommand(purchaseOrderId, request.InventoryItemId, request.QuantityOrdered, request.User, DateTimeOffset.UtcNow);
        var result = await purchaseOrderService.AddLineAsync(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpDelete("{purchaseOrderId:guid}/lines/{purchaseOrderLineId:guid}")]
    public async Task<ActionResult<ApiResponse<PurchaseOrderResponse>>> RemoveLine(Guid purchaseOrderId, Guid purchaseOrderLineId, [FromBody] RemovePurchaseOrderLineRequest request, CancellationToken cancellationToken)
    {
        var command = new RemovePurchaseOrderLineCommand(purchaseOrderId, purchaseOrderLineId, request.User, DateTimeOffset.UtcNow);
        var result = await purchaseOrderService.RemoveLineAsync(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPut("{purchaseOrderId:guid}/lines/{purchaseOrderLineId:guid}")]
    public async Task<ActionResult<ApiResponse<PurchaseOrderResponse>>> UpdateLine(Guid purchaseOrderId, Guid purchaseOrderLineId, [FromBody] UpdatePurchaseOrderLineRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdatePurchaseOrderLineCommand(purchaseOrderId, purchaseOrderLineId, request.QuantityOrdered, request.User, DateTimeOffset.UtcNow);
        var result = await purchaseOrderService.UpdateLineAsync(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPut("{purchaseOrderId:guid}/approve")]
    public async Task<ActionResult<ApiResponse<PurchaseOrderResponse>>> Approve(Guid purchaseOrderId, [FromBody] ChangePurchaseOrderStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await purchaseOrderService.ApproveAsync(new ChangePurchaseOrderStatusCommand(purchaseOrderId, request.User, DateTimeOffset.UtcNow), cancellationToken);
        return result.ToActionResult();
    }

    [HttpPut("{purchaseOrderId:guid}/close")]
    public async Task<ActionResult<ApiResponse<PurchaseOrderResponse>>> Close(Guid purchaseOrderId, [FromBody] ChangePurchaseOrderStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await purchaseOrderService.CloseAsync(new ChangePurchaseOrderStatusCommand(purchaseOrderId, request.User, DateTimeOffset.UtcNow), cancellationToken);
        return result.ToActionResult();
    }

    [HttpPut("{purchaseOrderId:guid}/cancel")]
    public async Task<ActionResult<ApiResponse<PurchaseOrderResponse>>> Cancel(Guid purchaseOrderId, [FromBody] ChangePurchaseOrderStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await purchaseOrderService.CancelAsync(new ChangePurchaseOrderStatusCommand(purchaseOrderId, request.User, DateTimeOffset.UtcNow), cancellationToken);
        return result.ToActionResult();
    }
}
