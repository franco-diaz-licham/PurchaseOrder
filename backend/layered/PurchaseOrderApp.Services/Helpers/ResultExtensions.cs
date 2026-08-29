using Microsoft.AspNetCore.Mvc;
using PurchaseOrderApp.BL.Common;
using PurchaseOrderApp.Services.Models;

namespace PurchaseOrderApp.Services.Helpers;

public static class ResultExtensions
{
    public static ActionResult ToActionResult(this Result result)
    {
        if (result.IsSuccess) return new OkObjectResult(new ApiResponse(StatusCodes.Status200OK, "Success"));

        return result.Status switch {
            ResultStatus.Invalid => new BadRequestObjectResult(new ApiResponse(StatusCodes.Status400BadRequest, result.Error ?? "Invalid request.")),
            ResultStatus.NotFound => new NotFoundObjectResult(new ApiResponse(StatusCodes.Status404NotFound, result.Error ?? "Not found.")),
            _ => new BadRequestObjectResult(new ApiResponse(StatusCodes.Status400BadRequest, result.Error ?? "Request failed."))
        };
    }

    public static ActionResult ToActionResult<T>(this Result<T> result, string? locationUrl = null)
    {
        if (!result.IsSuccess) {
            return result.Status switch {
                ResultStatus.Invalid => new BadRequestObjectResult(new ApiResponse(StatusCodes.Status400BadRequest, result.Error ?? "Invalid request.")),
                ResultStatus.NotFound => new NotFoundObjectResult(new ApiResponse(StatusCodes.Status404NotFound, result.Error ?? "Not found.")),
                _ => new BadRequestObjectResult(new ApiResponse(StatusCodes.Status400BadRequest, result.Error ?? "Request failed."))
            };
        }

        if (result.Status == ResultStatus.Created && !string.IsNullOrWhiteSpace(locationUrl)) {
            return new CreatedResult(locationUrl, new ApiResponse<T>(StatusCodes.Status201Created, "Created", result.Value!));
        }

        return new OkObjectResult(new ApiResponse<T>(StatusCodes.Status200OK, "Success", result.Value!));
    }
}
