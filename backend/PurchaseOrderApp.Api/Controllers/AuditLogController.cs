using Microsoft.AspNetCore.Mvc;
using PurchaseOrderApp.Api.Helpers;
using PurchaseOrderApp.Api.Models;
using PurchaseOrderApp.Application.Models;
using PurchaseOrderApp.Application.UseCases;
using PurchaseOrderApp.Domain.ValueObjects;

namespace PurchaseOrderApp.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class AuditLogController(IAuditLogService auditLogService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<AuditLogResponse>>>> GetAll([FromQuery] Guid? warehouseId, CancellationToken cancellationToken)
    {
        WarehouseId? parsedWarehouseId = warehouseId.HasValue ? new WarehouseId(warehouseId.Value) : null;
        var result = await auditLogService.ListAsync(parsedWarehouseId, cancellationToken);
        return result.ToActionResult();
    }
}
