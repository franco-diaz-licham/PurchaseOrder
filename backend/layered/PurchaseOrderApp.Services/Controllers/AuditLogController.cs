using Microsoft.AspNetCore.Mvc;
using PurchaseOrderApp.BL.Queries.AuditLogs;
using PurchaseOrderApp.BL.Responses;
using PurchaseOrderApp.Services.Helpers;
using PurchaseOrderApp.Services.Models;

namespace PurchaseOrderApp.Services.Controllers;

[ApiController]
[Route("api/audit-log")]
public sealed class AuditLogController(ListAuditLogEntriesQueryHandler listAuditLogEntries) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<AuditLogResponse>>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await listAuditLogEntries.ExecuteAsync(new ListAuditLogEntriesQuery(), cancellationToken);
        return result.ToActionResult();
    }
}
