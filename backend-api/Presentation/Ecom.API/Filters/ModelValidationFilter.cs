using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Ecom.Application.Common.Models;
using Ecom.Domain.Constants;

namespace Ecom.API.Filters;

public class ModelValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var validationErrors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                );

            var errorMessages = validationErrors
                .SelectMany(kvp => kvp.Value.Select(error => $"{kvp.Key}: {error}"))
                .ToList();

            var message = errorMessages.Count == 1 
                ? errorMessages.First()
                : $"Multiple validation errors occurred: {string.Join("; ", errorMessages)}";

            var response = ApiResponse<object>.Fail(
                message: message,
                errorCode: ErrorCodes.BAD_REQUEST,
                validationErrors: validationErrors
            );

            context.Result = new BadRequestObjectResult(response);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // No implementation needed
    }
}

