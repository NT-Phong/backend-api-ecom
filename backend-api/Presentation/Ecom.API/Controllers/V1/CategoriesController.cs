using Ecom.Application.Features.Catalog.Queries.GetPublicCategories;
using Microsoft.AspNetCore.Authorization;

namespace Ecom.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/categories")]
[AllowAnonymous]
public sealed class CategoriesController : BaseController
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken) =>
        HandleResult(await Mediator.Send(new GetPublicCategoriesQuery(), cancellationToken));
}
