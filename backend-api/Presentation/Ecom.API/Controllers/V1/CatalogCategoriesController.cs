using Ecom.Application.Features.Catalog.Categories;
using Microsoft.AspNetCore.Authorization;

namespace Ecom.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/catalog/categories")]
[Authorize]
public sealed class CatalogCategoriesController : BaseController
{
    [HttpGet]
    [Authorize(Policy = Permissions.CatalogCategories.Read)]
    public async Task<IActionResult> GetList([FromQuery] GetCatalogCategoryListQuery query, CancellationToken ct) => HandleResult(await Mediator.Send(query, ct));

    [HttpGet("{categoryId:guid}")]
    [Authorize(Policy = Permissions.CatalogCategories.Read)]
    public async Task<IActionResult> GetById(Guid categoryId, CancellationToken ct) => HandleResult(await Mediator.Send(new GetCatalogCategoryByIdQuery(categoryId), ct));

    [HttpGet("tree")]
    [Authorize(Policy = Permissions.CatalogCategories.Read)]
    public async Task<IActionResult> GetTree(CancellationToken ct) => HandleResult(await Mediator.Send(new GetCatalogCategoryTreeQuery(), ct));

    [HttpPost]
    [Authorize(Policy = Permissions.CatalogCategories.Create)]
    public async Task<IActionResult> Create(CreateCatalogCategoryCommand command, CancellationToken ct) => HandleResult(await Mediator.Send(command, ct));

    [HttpPut("{categoryId:guid}")]
    [Authorize(Policy = Permissions.CatalogCategories.Update)]
    public async Task<IActionResult> Update(Guid categoryId, UpdateCatalogCategoryCommand command, CancellationToken ct) => HandleResult(await Mediator.Send(command with { CategoryId = categoryId }, ct));

    [HttpPost("{categoryId:guid}/publish")]
    [Authorize(Policy = Permissions.CatalogCategories.Publish)]
    public async Task<IActionResult> Publish(Guid categoryId, PublishCatalogCategoryCommand command, CancellationToken ct) => HandleResult(await Mediator.Send(command with { CategoryId = categoryId }, ct));

    [HttpPost("{categoryId:guid}/pause")]
    [Authorize(Policy = Permissions.CatalogCategories.Publish)]
    public async Task<IActionResult> Pause(Guid categoryId, PauseCatalogCategoryCommand command, CancellationToken ct) => HandleResult(await Mediator.Send(command with { CategoryId = categoryId }, ct));

    [HttpDelete("{categoryId:guid}")]
    [Authorize(Policy = Permissions.CatalogCategories.Deactivate)]
    public async Task<IActionResult> Hide(Guid categoryId, HideCatalogCategoryCommand command, CancellationToken ct) => HandleResult(await Mediator.Send(command with { CategoryId = categoryId }, ct));
}
