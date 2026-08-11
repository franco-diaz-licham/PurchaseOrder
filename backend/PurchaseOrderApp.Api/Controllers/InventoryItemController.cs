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
public sealed class InventoryItemController(IInventoryItemService inventoryItemService) : ControllerBase
{
    [HttpPut("{inventoryItemId:guid}/standard-cost")]
    public async Task<ActionResult<ApiResponse>> ChangeStandardCost(Guid inventoryItemId, [FromBody] ChangeInventoryItemStandardCostRequest request, CancellationToken cancellationToken)
    {
        var command = new ChangeInventoryItemStandardCostCommand(
            new InventoryItemId(inventoryItemId),
            new Money(request.StandardCost),
            request.User,
            DateTimeOffset.UtcNow);

        var result = await inventoryItemService.ChangeStandardCostAsync(command, cancellationToken);
        return result.ToActionResult();
    }
}
