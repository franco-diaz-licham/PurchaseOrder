using Microsoft.AspNetCore.Mvc;
using PurchaseOrderApp.Api.Models;

namespace PurchaseOrderApp.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public ActionResult<ApiResponse> Get()
    {
        return Ok(new ApiResponse(StatusCodes.Status200OK, "Healthy"));
    }
}
