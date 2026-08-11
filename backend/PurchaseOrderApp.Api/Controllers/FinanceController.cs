using Microsoft.AspNetCore.Mvc;
using PurchaseOrderApp.Api.Helpers;
using PurchaseOrderApp.Api.Models;
using PurchaseOrderApp.Application.Models;
using PurchaseOrderApp.Application.UseCases;

namespace PurchaseOrderApp.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class FinanceController(IFinanceService financeService) : ControllerBase
{
    [HttpGet("warehouse-committed-values")]
    public async Task<ActionResult<ApiResponse<List<WarehouseCommittedStockValueResponse>>>> GetWarehouseCommittedValues(CancellationToken cancellationToken)
    {
        var result = await financeService.ListWarehouseCommittedStockValuesAsync(cancellationToken);
        return result.ToActionResult();
    }
}
