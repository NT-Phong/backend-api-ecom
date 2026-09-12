using Ecom.Application.Features.Catalog.Queries.GetProductBySlug;
using Ecom.Application.Features.Catalog.Queries.GetProductList;
using Ecom.Application.Features.Catalog.Common;
using Microsoft.AspNetCore.Authorization;

namespace Ecom.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/products")]
[AllowAnonymous]
public sealed class ProductsController : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] GetProductListQuery query, CancellationToken cancellationToken) =>
        HandleResult(await Mediator.Send(query, cancellationToken));

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetProductBySlugQuery(slug), cancellationToken);
        if (!result.IsSuccess)
            return HandleResult(result);
        if (result.Data.IsRedirect)
        {
            var prefix = HttpContext.Request.Path.Value!.TrimEnd('/');
            var lastSlash = prefix.LastIndexOf('/');
            return RedirectPermanent($"{prefix[..(lastSlash + 1)]}{Uri.EscapeDataString(result.Data.CanonicalSlug!)}");
        }
        return Ok(ApiResponse<ProductDetailDto>.Ok(result.Data.Product!));
    }
}
