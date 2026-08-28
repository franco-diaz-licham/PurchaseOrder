using Microsoft.AspNetCore.Mvc;
using PurchaseOrderApp.BL.Commands;
using PurchaseOrderApp.BL.Common;
using PurchaseOrderApp.BL.Workflows;
using PurchaseOrderApp.Services.Models;

namespace PurchaseOrderApp.Services.Controllers;

[ApiController]
[Route("api/purchase-orders")]
public sealed class PurchaseOrdersController(IPurchaseOrderWorkflowService purchaseOrderWorkflowService) : ControllerBase
{
    [HttpPost("{purchaseOrderId:guid}/approve")]
    public async Task<IActionResult> Approve(
        Guid purchaseOrderId,
        [FromBody] ChangePurchaseOrderStatusRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ChangePurchaseOrderStatusCommand(
            purchaseOrderId,
            request.User,
            DateTimeOffset.UtcNow);

        var result = await purchaseOrderWorkflowService.ApproveAsync(command, cancellationToken);
        return ToActionResult(result);
    }

    private IActionResult ToActionResult(Result result)
    {
        if (result.IsSuccess) return NoContent();

        return result.Status switch {
            ResultStatus.NotFound => NotFound(new { error = result.Error }),
            _ => BadRequest(new { error = result.Error })
        };
    }
}
