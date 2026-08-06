using Ecom.Application.Features.Catalog.Commands.ChangeProductLifecycle;
using Ecom.Application.Features.Catalog.Commands.ChangeProductVariantLifecycle;
using Ecom.Application.Features.Catalog.Products.Commands.CreateProduct;
using Ecom.Application.Features.Catalog.Commands.CreateVariantPrice;
using Ecom.Application.Features.Catalog.Commands.ManageProductMedia;
using Ecom.Application.Features.Catalog.Commands.ManageProductVariants;
using Ecom.Application.Features.Catalog.Commands.ReplaceProductCategories;
using Ecom.Application.Features.Catalog.Products.Commands.UpdateProductDetails;
using Ecom.Application.Features.Catalog.Queries.GetCatalogProductById;
using Ecom.Application.Features.Catalog.Queries.GetCatalogProductList;
using Microsoft.AspNetCore.Authorization;

namespace Ecom.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/catalog/products")]
[Authorize]
public sealed class CatalogProductsController : BaseController
{
    [HttpGet]
    [Authorize(Policy = Permissions.CatalogProducts.Read)]
    public async Task<IActionResult> GetList([FromQuery] GetCatalogProductListQuery query, CancellationToken cancellationToken) =>
        HandleResult(await Mediator.Send(query, cancellationToken));

    [HttpGet("{productId:guid}")]
    [Authorize(Policy = Permissions.CatalogProducts.Read)]
    public async Task<IActionResult> GetById(Guid productId, CancellationToken cancellationToken) =>
        HandleResult(await Mediator.Send(new GetCatalogProductByIdQuery(productId), cancellationToken));

    [HttpPost]
    [Authorize(Policy = Permissions.CatalogProducts.Create)]
    public async Task<IActionResult> Create([FromBody] CreateProductCommand command, CancellationToken cancellationToken) =>
        HandleResult(await Mediator.Send(command, cancellationToken));

    [HttpPut("{productId:guid}")]
    [Authorize(Policy = Permissions.CatalogProducts.Update)]
    public async Task<IActionResult> Update(Guid productId, [FromBody] UpdateProductDetailsCommand command, CancellationToken cancellationToken) =>
        HandleResult(await Mediator.Send(command with { ProductId = productId }, cancellationToken));

    [HttpPut("{productId:guid}/categories")]
    [Authorize(Policy = Permissions.CatalogProducts.Update)]
    public async Task<IActionResult> ReplaceCategories(Guid productId, [FromBody] ReplaceProductCategoriesCommand command, CancellationToken cancellationToken) =>
        HandleResult(await Mediator.Send(command with { ProductId = productId }, cancellationToken));

    [HttpPost("{productId:guid}/media")]
    [Authorize(Policy = Permissions.CatalogProducts.Update)]
    public async Task<IActionResult> AttachMedia(Guid productId, [FromBody] AttachProductMediaCommand command, CancellationToken cancellationToken) =>
        HandleResult(await Mediator.Send(command with { ProductId = productId }, cancellationToken));

    [HttpPatch("{productId:guid}/media/{mediaAssetId:guid}")]
    [Authorize(Policy = Permissions.CatalogProducts.Update)]
    public async Task<IActionResult> UpdateMedia(Guid productId, Guid mediaAssetId, [FromBody] UpdateProductMediaCommand command, CancellationToken cancellationToken) =>
        HandleResult(await Mediator.Send(command with { ProductId = productId, MediaAssetId = mediaAssetId }, cancellationToken));

    [HttpPost("{productId:guid}/media/{mediaAssetId:guid}/primary")]
    [Authorize(Policy = Permissions.CatalogProducts.Update)]
    public async Task<IActionResult> SetPrimaryMedia(Guid productId, Guid mediaAssetId, [FromBody] SetPrimaryProductMediaCommand command, CancellationToken cancellationToken) =>
        HandleResult(await Mediator.Send(command with { ProductId = productId, MediaAssetId = mediaAssetId }, cancellationToken));

    [HttpDelete("{productId:guid}/media/{mediaAssetId:guid}")]
    [Authorize(Policy = Permissions.CatalogProducts.Update)]
    public async Task<IActionResult> RemoveMedia(Guid productId, Guid mediaAssetId, [FromBody] RemoveProductMediaCommand command, CancellationToken cancellationToken) =>
        HandleResult(await Mediator.Send(command with { ProductId = productId, MediaAssetId = mediaAssetId }, cancellationToken));

    [HttpPost("{productId:guid}/variants")]
    [Authorize(Policy = Permissions.CatalogProducts.Update)]
    public async Task<IActionResult> CreateVariant(Guid productId, [FromBody] CreateProductVariantCommand command, CancellationToken cancellationToken) =>
        HandleResult(await Mediator.Send(command with { ProductId = productId }, cancellationToken));

    [HttpPut("{productId:guid}/variants/{variantId:guid}")]
    [Authorize(Policy = Permissions.CatalogProducts.Update)]
    public async Task<IActionResult> UpdateVariant(Guid productId, Guid variantId, [FromBody] UpdateProductVariantCommand command, CancellationToken cancellationToken) =>
        HandleResult(await Mediator.Send(command with { ProductId = productId, VariantId = variantId }, cancellationToken));

    [HttpPost("{productId:guid}/variants/{variantId:guid}/pause")]
    [Authorize(Policy = Permissions.CatalogProducts.Update)]
    public async Task<IActionResult> PauseVariant(Guid productId, Guid variantId, [FromBody] PauseProductVariantCommand command, CancellationToken cancellationToken) =>
        HandleResult(await Mediator.Send(command with { ProductId = productId, VariantId = variantId }, cancellationToken));

    [HttpPost("{productId:guid}/variants/{variantId:guid}/activate")]
    [Authorize(Policy = Permissions.CatalogProducts.Update)]
    public async Task<IActionResult> ActivateVariant(Guid productId, Guid variantId, [FromBody] ActivateProductVariantCommand command, CancellationToken cancellationToken) =>
        HandleResult(await Mediator.Send(command with { ProductId = productId, VariantId = variantId }, cancellationToken));

    [HttpPost("{productId:guid}/variants/{variantId:guid}/discontinue")]
    [Authorize(Policy = Permissions.CatalogProducts.Update)]
    public async Task<IActionResult> DiscontinueVariant(Guid productId, Guid variantId, [FromBody] DiscontinueProductVariantCommand command, CancellationToken cancellationToken) =>
        HandleResult(await Mediator.Send(command with { ProductId = productId, VariantId = variantId }, cancellationToken));

    [HttpPost("{productId:guid}/variants/{variantId:guid}/prices")]
    [Authorize(Policy = Permissions.CatalogProducts.Update)]
    public async Task<IActionResult> CreatePrice(Guid productId, Guid variantId, [FromBody] CreateVariantPriceCommand command, CancellationToken cancellationToken) =>
        HandleResult(await Mediator.Send(command with { ProductId = productId, VariantId = variantId }, cancellationToken));

    [HttpPost("{productId:guid}/submit-review")]
    [Authorize(Policy = Permissions.CatalogProducts.Publish)]
    public async Task<IActionResult> SubmitForReview(Guid productId, [FromBody] SubmitProductForReviewCommand command, CancellationToken cancellationToken) =>
        HandleResult(await Mediator.Send(command with { ProductId = productId }, cancellationToken));

    [HttpPost("{productId:guid}/publish")]
    [Authorize(Policy = Permissions.CatalogProducts.Publish)]
    public async Task<IActionResult> Publish(Guid productId, [FromBody] PublishProductCommand command, CancellationToken cancellationToken) =>
        HandleResult(await Mediator.Send(command with { ProductId = productId }, cancellationToken));

    [HttpPost("{productId:guid}/pause")]
    [Authorize(Policy = Permissions.CatalogProducts.Publish)]
    public async Task<IActionResult> Pause(Guid productId, [FromBody] PauseProductCommand command, CancellationToken cancellationToken) =>
        HandleResult(await Mediator.Send(command with { ProductId = productId }, cancellationToken));

    [HttpPost("{productId:guid}/discontinue")]
    [Authorize(Policy = Permissions.CatalogProducts.Discontinue)]
    public async Task<IActionResult> Discontinue(Guid productId, [FromBody] DiscontinueProductCommand command, CancellationToken cancellationToken) =>
        HandleResult(await Mediator.Send(command with { ProductId = productId }, cancellationToken));

    [HttpDelete("{productId:guid}")]
    [Authorize(Policy = Permissions.CatalogProducts.Discontinue)]
    public async Task<IActionResult> Delete(Guid productId, [FromBody] DiscontinueProductCommand command, CancellationToken cancellationToken) =>
        HandleResult(await Mediator.Send(command with { ProductId = productId }, cancellationToken));
}
