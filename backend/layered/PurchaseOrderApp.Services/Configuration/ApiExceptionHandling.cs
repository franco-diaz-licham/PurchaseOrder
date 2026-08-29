using PurchaseOrderApp.Services.Models;

namespace PurchaseOrderApp.Services.Configuration;

public static class ApiExceptionHandling
{
    public static WebApplication UseApiExceptionHandling(this WebApplication app)
    {
        app.Use(async (context, next) => {
            try {
                await next();
            } catch (InvalidOperationException ex) {
                await WriteErrorAsync(context, StatusCodes.Status400BadRequest, ex.Message);
            } catch {
                await WriteErrorAsync(context, StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        });

        return app;
    }

    private static Task WriteErrorAsync(HttpContext context, int statusCode, string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        return context.Response.WriteAsJsonAsync(new ApiResponse(statusCode, message));
    }
}
