using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PurchaseOrderApp.Services.Models;

namespace PurchaseOrderApp.Services.Validation;

public sealed class RequestValidationFilter(IServiceProvider serviceProvider) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values) {
            if (argument is null) continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (serviceProvider.GetService(validatorType) is not IValidator validator) continue;

            var validationContext = new ValidationContext<object>(argument);
            var result = await validator.ValidateAsync(validationContext, context.HttpContext.RequestAborted);
            if (result.IsValid) continue;

            var message = string.Join(" ", result.Errors.Select(error => error.ErrorMessage).Distinct());
            context.Result = new BadRequestObjectResult(new ApiResponse(StatusCodes.Status400BadRequest, message));
            return;
        }

        await next();
    }
}
