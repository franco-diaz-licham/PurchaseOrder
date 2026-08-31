using Microsoft.AspNetCore.Mvc;
using PurchaseOrderApp.BL.Commands;
using PurchaseOrderApp.BL.Commands.PurchaseOrders;
using PurchaseOrderApp.BL.Queries.PurchaseOrders;
using PurchaseOrderApp.BL.Responses;
using PurchaseOrderApp.Services.Helpers;
using PurchaseOrderApp.Services.Models;

namespace PurchaseOrderApp.Services.Controllers;

[ApiController]
[Route("api/purchase-order")]
public sealed class PurchaseOrderController(
    GetPurchaseOrderQueryHandler getPurchaseOrder,
    ListPurchaseOrderSummariesQueryHandler listPurchaseOrderSummaries,
    SubmitPurchaseOrderCommandHandler submitPurchaseOrder,
    AddPurchaseOrderLineCommandHandler addPurchaseOrderLine,
    UpdatePurchaseOrderLineCommandHandler updatePurchaseOrderLine,
    RemovePurchaseOrderLineCommandHandler removePurchaseOrderLine,
    ApprovePurchaseOrderCommandHandler approvePurchaseOrder,
    ClosePurchaseOrderCommandHandler closePurchaseOrder,
    CancelPurchaseOrderCommandHandler cancelPurchaseOrder) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<ActionResult<ApiResponse<List<PurchaseOrderSummaryResponse>>>> GetSummary(CancellationToken cancellationToken)
    {
        var result = await listPurchaseOrderSummaries.ExecuteAsync(new ListPurchaseOrderSummariesQuery(), cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("{purchaseOrderId:guid}")]
    public async Task<ActionResult<ApiResponse<PurchaseOrderResponse>>> Get(Guid purchaseOrderId, CancellationToken cancellationToken)
    {
        var result = await getPurchaseOrder.ExecuteAsync(new GetPurchaseOrderQuery(purchaseOrderId), cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PurchaseOrderResponse>>> Submit([FromBody] SubmitPurchaseOrderRequest request, CancellationToken cancellationToken)
    {
        var lines = request.Lines?.Select(line => new SubmitPurchaseOrderLineCommand(line.InventoryItemId, line.QuantityOrdered)).ToList() ?? [];
        var command = new SubmitPurchaseOrderCommand(request.WarehouseId, lines, request.User, DateTimeOffset.UtcNow);
        var result = await submitPurchaseOrder.ExecuteAsync(command, cancellationToken);
        return result.ToActionResult($"/api/purchase-order/{result.Value?.PurchaseOrderId}");
    }

    [HttpPost("{purchaseOrderId:guid}/lines")]
    public async Task<ActionResult<ApiResponse<PurchaseOrderResponse>>> AddLine(Guid purchaseOrderId, [FromBody] AddPurchaseOrderLineRequest request, CancellationToken cancellationToken)
    {
        var command = new AddPurchaseOrderLineCommand(purchaseOrderId, request.InventoryItemId, request.QuantityOrdered, request.User, DateTimeOffset.UtcNow);
        var result = await addPurchaseOrderLine.ExecuteAsync(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpDelete("{purchaseOrderId:guid}/lines/{purchaseOrderLineId:guid}")]
    public async Task<ActionResult<ApiResponse<PurchaseOrderResponse>>> RemoveLine(Guid purchaseOrderId, Guid purchaseOrderLineId, [FromBody] RemovePurchaseOrderLineRequest request, CancellationToken cancellationToken)
    {
        var command = new RemovePurchaseOrderLineCommand(purchaseOrderId, purchaseOrderLineId, request.User, DateTimeOffset.UtcNow);
        var result = await removePurchaseOrderLine.ExecuteAsync(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPut("{purchaseOrderId:guid}/lines/{purchaseOrderLineId:guid}")]
    public async Task<ActionResult<ApiResponse<PurchaseOrderResponse>>> UpdateLine(Guid purchaseOrderId, Guid purchaseOrderLineId, [FromBody] UpdatePurchaseOrderLineRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdatePurchaseOrderLineCommand(purchaseOrderId, purchaseOrderLineId, request.QuantityOrdered, request.User, DateTimeOffset.UtcNow);
        var result = await updatePurchaseOrderLine.ExecuteAsync(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPut("{purchaseOrderId:guid}/approve")]
    public async Task<ActionResult<ApiResponse<PurchaseOrderResponse>>> Approve(Guid purchaseOrderId, [FromBody] ChangePurchaseOrderStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await approvePurchaseOrder.ExecuteAsync(new ApprovePurchaseOrderCommand(purchaseOrderId, request.User, DateTimeOffset.UtcNow), cancellationToken);
        return result.ToActionResult();
    }

    [HttpPut("{purchaseOrderId:guid}/close")]
    public async Task<ActionResult<ApiResponse<PurchaseOrderResponse>>> Close(Guid purchaseOrderId, [FromBody] ChangePurchaseOrderStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await closePurchaseOrder.ExecuteAsync(new ClosePurchaseOrderCommand(purchaseOrderId, request.User, DateTimeOffset.UtcNow), cancellationToken);
        return result.ToActionResult();
    }

    [HttpPut("{purchaseOrderId:guid}/cancel")]
    public async Task<ActionResult<ApiResponse<PurchaseOrderResponse>>> Cancel(Guid purchaseOrderId, [FromBody] ChangePurchaseOrderStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await cancelPurchaseOrder.ExecuteAsync(new CancelPurchaseOrderCommand(purchaseOrderId, request.User, DateTimeOffset.UtcNow), cancellationToken);
        return result.ToActionResult();
    }
}
