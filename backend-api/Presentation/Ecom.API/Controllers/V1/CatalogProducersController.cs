using Ecom.Application.Features.Catalog.Producers.Queries.GetCatalogProducerById;
using Ecom.Application.Features.Catalog.Producers.Queries.GetCatalogProducerList;
using Microsoft.AspNetCore.Authorization;

namespace Ecom.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/catalog/producers")]
[Authorize]
public sealed class CatalogProducersController : BaseController
{
    [HttpGet]
    [Authorize(Policy = Permissions.CatalogProducts.Create)]
    public async Task<IActionResult> GetList(
        [FromQuery] GetCatalogProducerListQuery query,
        CancellationToken cancellationToken) =>
        HandleResult(await Mediator.Send(query, cancellationToken));

    [HttpGet("{producerId:guid}")]
    [Authorize(Policy = Permissions.CatalogProducts.Create)]
    public async Task<IActionResult> GetById(Guid producerId, CancellationToken cancellationToken) =>
        HandleResult(await Mediator.Send(new GetCatalogProducerByIdQuery(producerId), cancellationToken));
}
