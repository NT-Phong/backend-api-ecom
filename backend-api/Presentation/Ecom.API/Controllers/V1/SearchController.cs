using Ecom.Application.Features.Catalog.Queries.SearchProducts;
using Microsoft.AspNetCore.Authorization;

namespace Ecom.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/search")]
[AllowAnonymous]
public sealed class SearchController : BaseController
{
    [HttpGet("products")]
    public async Task<IActionResult> Products([FromQuery] SearchProductsQuery query, CancellationToken cancellationToken) =>
        HandleResult(await Mediator.Send(query, cancellationToken));

    [HttpGet("suggestions")]
    public async Task<IActionResult> Suggestions([FromQuery] ProductSearchSuggestionsQuery query,
        CancellationToken cancellationToken) => HandleResult(await Mediator.Send(query, cancellationToken));
}
