using Microsoft.AspNetCore.Mvc;
using PurchaseOrderApp.BL.Responses;
using PurchaseOrderApp.BL.Workflows;
using PurchaseOrderApp.Services.Helpers;
using PurchaseOrderApp.Services.Models;

namespace PurchaseOrderApp.Services.Controllers;

[ApiController]
[Route("api/warehouse")]
public sealed class WarehouseController(IWarehouseService warehouseService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<WarehouseResponse>>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await warehouseService.ListAsync(cancellationToken);
        return result.ToActionResult();
    }
}
