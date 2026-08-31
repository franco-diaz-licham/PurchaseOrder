using Microsoft.AspNetCore.Mvc;
using PurchaseOrderApp.BL.Queries.Warehouses;
using PurchaseOrderApp.BL.Responses;
using PurchaseOrderApp.Services.Helpers;
using PurchaseOrderApp.Services.Models;

namespace PurchaseOrderApp.Services.Controllers;

[ApiController]
[Route("api/warehouse-stock")]
public sealed class WarehouseStockController(ListWarehouseStockQueryHandler listWarehouseStock) : ControllerBase
{
    [HttpGet("warehouse/{warehouseId:guid}")]
    public async Task<ActionResult<ApiResponse<List<WarehouseStockResponse>>>> GetForWarehouse(Guid warehouseId, CancellationToken cancellationToken)
    {
        var result = await listWarehouseStock.ExecuteAsync(new ListWarehouseStockQuery(warehouseId), cancellationToken);
        return result.ToActionResult();
    }
}
