using Microsoft.AspNetCore.Mvc;
using PurchaseOrderApp.BL.Responses;
using PurchaseOrderApp.BL.Workflows;
using PurchaseOrderApp.Services.Helpers;
using PurchaseOrderApp.Services.Models;

namespace PurchaseOrderApp.Services.Controllers;

[ApiController]
[Route("api/finance")]
public sealed class FinanceController(IFinanceService financeService) : ControllerBase
{
    [HttpGet("report")]
    public async Task<ActionResult<ApiResponse<List<WarehouseCommittedStockValueResponse>>>> GetReport(CancellationToken cancellationToken)
    {
        var result = await financeService.ListWarehouseCommittedStockValuesAsync(cancellationToken);
        return result.ToActionResult();
    }
}
