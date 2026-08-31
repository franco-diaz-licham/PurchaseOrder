using Microsoft.AspNetCore.Mvc;
using PurchaseOrderApp.BL.Commands;
using PurchaseOrderApp.BL.Commands.Inventory;
using PurchaseOrderApp.BL.Queries.Inventory;
using PurchaseOrderApp.BL.Responses;
using PurchaseOrderApp.Services.Helpers;
using PurchaseOrderApp.Services.Models;

namespace PurchaseOrderApp.Services.Controllers;

[ApiController]
[Route("api/inventory-item")]
public sealed class InventoryItemController(
    ListInventoryItemsQueryHandler listInventoryItems,
    ChangeInventoryItemStandardCostCommandHandler changeInventoryItemStandardCost) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<InventoryItemResponse>>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await listInventoryItems.ExecuteAsync(new ListInventoryItemsQuery(), cancellationToken);
        return result.ToActionResult();
    }

    [HttpPut("{inventoryItemId:guid}/standard-cost")]
    public async Task<ActionResult<ApiResponse>> ChangeStandardCost(Guid inventoryItemId, [FromBody] ChangeInventoryItemStandardCostRequest request, CancellationToken cancellationToken)
    {
        var command = new ChangeInventoryItemStandardCostCommand(inventoryItemId, request.StandardCost, request.User, DateTimeOffset.UtcNow);
        var result = await changeInventoryItemStandardCost.ExecuteAsync(command, cancellationToken);
        return result.ToActionResult();
    }
}
