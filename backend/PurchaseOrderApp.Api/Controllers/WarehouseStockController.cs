using Microsoft.AspNetCore.Mvc;
using PurchaseOrderApp.Api.Helpers;
using PurchaseOrderApp.Api.Models;
using PurchaseOrderApp.Application.Models;
using PurchaseOrderApp.Application.UseCases;
using PurchaseOrderApp.Domain.ValueObjects;

namespace PurchaseOrderApp.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class WarehouseStockController(IWarehouseStockService warehouseStockService) : ControllerBase
{
    [HttpGet("warehouse/{warehouseId:guid}")]
    public async Task<ActionResult<ApiResponse<List<WarehouseStockResponse>>>> GetForWarehouse(Guid warehouseId, CancellationToken cancellationToken)
    {
        var result = await warehouseStockService.ListAsync(new WarehouseId(warehouseId), cancellationToken);
        return result.ToActionResult();
    }
}
