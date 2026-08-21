using Ecom.Application.Common.Models;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Ecom.API.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class ValidateCommerceAntiforgeryTokenAttribute : TypeFilterAttribute
{
    public ValidateCommerceAntiforgeryTokenAttribute() : base(typeof(CommerceAntiforgeryFilter))
    {
    }
}

public sealed class CommerceAntiforgeryFilter(IAntiforgery antiforgery) : IAsyncAuthorizationFilter, IOrderedFilter
{
    public int Order => 1_000;

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        try
        {
            await antiforgery.ValidateRequestAsync(context.HttpContext);
        }
        catch (AntiforgeryValidationException)
        {
            context.HttpContext.Response.Headers["X-Trace-Id"] = context.HttpContext.TraceIdentifier;
            context.Result = new BadRequestObjectResult(ApiResponse<object>.Fail(
                "CSRF token is missing or invalid. Refresh it and try once.", "CSRF_INVALID"));
        }
    }
}
