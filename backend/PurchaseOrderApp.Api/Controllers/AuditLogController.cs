using Microsoft.AspNetCore.Mvc;
using PurchaseOrderApp.Api.Helpers;
using PurchaseOrderApp.Api.Models;
using PurchaseOrderApp.Application.Models;
using PurchaseOrderApp.Application.UseCases;

namespace PurchaseOrderApp.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class AuditLogController(IAuditLogService auditLogService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<AuditLogResponse>>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await auditLogService.ListAsync(cancellationToken);
        return result.ToActionResult();
    }
}
