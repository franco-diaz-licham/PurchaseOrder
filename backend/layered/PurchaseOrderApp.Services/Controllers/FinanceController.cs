using Microsoft.AspNetCore.Mvc;
using PurchaseOrderApp.BL.Queries.Finance;
using PurchaseOrderApp.BL.Responses;
using PurchaseOrderApp.Services.Helpers;
using PurchaseOrderApp.Services.Models;

namespace PurchaseOrderApp.Services.Controllers;

[ApiController]
[Route("api/finance")]
public sealed class FinanceController(ListWarehouseCommittedStockValuesQueryHandler listWarehouseCommittedStockValues) : ControllerBase
{
    [HttpGet("report")]
    public async Task<ActionResult<ApiResponse<List<WarehouseCommittedStockValueResponse>>>> GetReport(CancellationToken cancellationToken)
    {
        var result = await listWarehouseCommittedStockValues.ExecuteAsync(new ListWarehouseCommittedStockValuesQuery(), cancellationToken);
        return result.ToActionResult();
    }
}
