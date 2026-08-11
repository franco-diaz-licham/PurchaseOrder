using Microsoft.AspNetCore.Mvc;
using PurchaseOrderApp.Api.Helpers;
using PurchaseOrderApp.Api.Models;
using PurchaseOrderApp.Application.Models;
using PurchaseOrderApp.Application.UseCases;

namespace PurchaseOrderApp.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class WarehouseController(IWarehouseService warehouseService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<WarehouseResponse>>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await warehouseService.ListAsync(cancellationToken);
        return result.ToActionResult();
    }
}
