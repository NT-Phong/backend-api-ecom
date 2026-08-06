using Ecom.Application.Features.Catalog.Options;
using Microsoft.AspNetCore.Authorization;

namespace Ecom.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/catalog/products/{productId:guid}/options")]
[Authorize]
public sealed class CatalogProductOptionsController : BaseController
{
    [HttpGet]
    [Authorize(Policy = Permissions.CatalogProducts.Read)]
    public async Task<IActionResult> GetList(Guid productId, CancellationToken ct) => HandleResult(await Mediator.Send(new GetProductOptionsQuery(productId), ct));

    [HttpPost]
    [Authorize(Policy = Permissions.CatalogProducts.Update)]
    public async Task<IActionResult> Create(Guid productId, CreateProductOptionCommand command, CancellationToken ct) => HandleResult(await Mediator.Send(command with { ProductId = productId }, ct));
    [HttpPut("{optionId:guid}")]
    [Authorize(Policy = Permissions.CatalogProducts.Update)]
    public async Task<IActionResult> Update(Guid productId, Guid optionId, UpdateProductOptionCommand command, CancellationToken ct) => HandleResult(await Mediator.Send(command with { ProductId = productId, OptionId = optionId }, ct));
    [HttpDelete("{optionId:guid}")]
    [Authorize(Policy = Permissions.CatalogProducts.Update)]
    public async Task<IActionResult> Delete(Guid productId, Guid optionId, DeleteProductOptionCommand command, CancellationToken ct) => HandleResult(await Mediator.Send(command with { ProductId = productId, OptionId = optionId }, ct));
    [HttpPost("{optionId:guid}/values")]
    [Authorize(Policy = Permissions.CatalogProducts.Update)]
    public async Task<IActionResult> CreateValue(Guid productId, Guid optionId, CreateProductOptionValueCommand command, CancellationToken ct) => HandleResult(await Mediator.Send(command with { ProductId = productId, OptionId = optionId }, ct));
    [HttpPut("{optionId:guid}/values/{valueId:guid}")]
    [Authorize(Policy = Permissions.CatalogProducts.Update)]
    public async Task<IActionResult> UpdateValue(Guid productId, Guid optionId, Guid valueId, UpdateProductOptionValueCommand command, CancellationToken ct) => HandleResult(await Mediator.Send(command with { ProductId = productId, OptionId = optionId, ValueId = valueId }, ct));
    [HttpDelete("{optionId:guid}/values/{valueId:guid}")]
    [Authorize(Policy = Permissions.CatalogProducts.Update)]
    public async Task<IActionResult> DeleteValue(Guid productId, Guid optionId, Guid valueId, DeleteProductOptionValueCommand command, CancellationToken ct) => HandleResult(await Mediator.Send(command with { ProductId = productId, OptionId = optionId, ValueId = valueId }, ct));
    [HttpPut("~/api/v{version:apiVersion}/catalog/products/{productId:guid}/variants/{variantId:guid}/option-values")]
    [Authorize(Policy = Permissions.CatalogProducts.Update)]
    public async Task<IActionResult> ReplaceVariantValues(Guid productId, Guid variantId, ReplaceVariantOptionValuesCommand command, CancellationToken ct) => HandleResult(await Mediator.Send(command with { ProductId = productId, VariantId = variantId }, ct));
}
