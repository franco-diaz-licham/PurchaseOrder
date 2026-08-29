using Microsoft.AspNetCore.Mvc;
using PurchaseOrderApp.Services.Models;

namespace PurchaseOrderApp.Services.Controllers;

[ApiController]
[Route("api/health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public ActionResult<ApiResponse> Get()
    {
        return Ok(new ApiResponse(StatusCodes.Status200OK, "Healthy"));
    }
}
