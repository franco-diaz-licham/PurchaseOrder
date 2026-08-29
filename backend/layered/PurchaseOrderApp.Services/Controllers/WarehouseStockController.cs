using Microsoft.AspNetCore.Mvc;
using PurchaseOrderApp.BL.Responses;
using PurchaseOrderApp.BL.Workflows;
using PurchaseOrderApp.Services.Helpers;
using PurchaseOrderApp.Services.Models;

namespace PurchaseOrderApp.Services.Controllers;

[ApiController]
[Route("api/warehouse-stock")]
public sealed class WarehouseStockController(IWarehouseStockService warehouseStockService) : ControllerBase
{
    [HttpGet("warehouse/{warehouseId:guid}")]
    public async Task<ActionResult<ApiResponse<List<WarehouseStockResponse>>>> GetForWarehouse(Guid warehouseId, CancellationToken cancellationToken)
    {
        var result = await warehouseStockService.ListAsync(warehouseId, cancellationToken);
        return result.ToActionResult();
    }
}
