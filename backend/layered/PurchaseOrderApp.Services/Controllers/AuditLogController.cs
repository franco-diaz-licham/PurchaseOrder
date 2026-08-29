using Microsoft.AspNetCore.Mvc;
using PurchaseOrderApp.BL.Responses;
using PurchaseOrderApp.BL.Workflows;
using PurchaseOrderApp.Services.Helpers;
using PurchaseOrderApp.Services.Models;

namespace PurchaseOrderApp.Services.Controllers;

[ApiController]
[Route("api/audit-log")]
public sealed class AuditLogController(IAuditLogService auditLogService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<AuditLogResponse>>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await auditLogService.ListAsync(cancellationToken);
        return result.ToActionResult();
    }
}
