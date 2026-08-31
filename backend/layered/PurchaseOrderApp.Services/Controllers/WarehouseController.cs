using Microsoft.AspNetCore.Mvc;
using PurchaseOrderApp.BL.Queries.Warehouses;
using PurchaseOrderApp.BL.Responses;
using PurchaseOrderApp.Services.Helpers;
using PurchaseOrderApp.Services.Models;

namespace PurchaseOrderApp.Services.Controllers;

[ApiController]
[Route("api/warehouse")]
public sealed class WarehouseController(ListWarehousesQueryHandler listWarehouses) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<WarehouseResponse>>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await listWarehouses.ExecuteAsync(new ListWarehousesQuery(), cancellationToken);
        return result.ToActionResult();
    }
}
