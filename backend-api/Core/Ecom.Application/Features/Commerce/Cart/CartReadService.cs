using Ecom.Application.Common.Interfaces;
using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Cart;

/// <summary>
/// Builds the cart's public read model in bounded queries. This is shared by cart reads and
/// mutation responses so optional catalog fields cannot silently disappear after an update.
/// </summary>
public sealed class CartReadService(
    IUnitOfWork unitOfWork,
    IEffectivePriceResolver effectivePriceResolver,
    IProductMediaReader productMediaReader) : ICartReadService
{
    public async Task<CartDto> BuildAsync(Ecom.Domain.Entities.Cart cart, IReadOnlyCollection<CartItem> items,
        CancellationToken cancellationToken)
    {
        var activeItems = items.Where(x => !x.IsDeleted).OrderBy(x => x.CreatedAt).ToList();
        if (activeItems.Count == 0)
            return new CartDto(cart.Id, cart.Status, []);

        var variantIds = activeItems.Select(x => x.ProductVariantId).Distinct().ToArray();
        var rows = await (
            from variant in unitOfWork.Repository<ProductVariant>().QueryNoTracking()
            join product in unitOfWork.Repository<Product>().QueryNoTracking() on variant.ProductId equals product.Id
            where variantIds.Contains(variant.Id)
            select new CartCatalogRow(variant.Id, product.Id, product.Name, product.Slug, product.Status,
                variant.Sku, variant.Name, variant.Status, variant.UnitLabel))
            .ToListAsync(cancellationToken);
        var catalogByVariantId = rows.ToDictionary(x => x.VariantId);
        var prices = await effectivePriceResolver.ResolveForVariantsAsync(variantIds, DateTime.UtcNow,
            cancellationToken);
        var mediaByProductId = await productMediaReader.GetPrimaryCleanPublicMediaAsync(
            rows.Where(x => x.ProductStatus == ProductStatus.Published).Select(x => x.ProductId).Distinct().ToArray(),
            cancellationToken);

        var responseItems = activeItems.Select(item =>
        {
            if (!catalogByVariantId.TryGetValue(item.ProductVariantId, out var catalog))
                return new CartItemDto(item.Id, item.ProductVariantId, item.Quantity);

            var isPubliclyAvailable = catalog.ProductStatus == ProductStatus.Published &&
                                      catalog.VariantStatus == VariantStatus.Active;
            prices.TryGetValue(item.ProductVariantId, out var price);
            mediaByProductId.TryGetValue(catalog.ProductId, out var media);
            return new CartItemDto(item.Id, item.ProductVariantId, item.Quantity,
                isPubliclyAvailable ? catalog.ProductName : null,
                isPubliclyAvailable ? catalog.VariantName : null,
                isPubliclyAvailable ? catalog.Sku : null,
                isPubliclyAvailable && price is not null ? price.Amount : null,
                isPubliclyAvailable ? media?.Url : null,
                isPubliclyAvailable ? catalog.Slug : null,
                isPubliclyAvailable ? catalog.UnitLabel : null);
        }).ToList();

        return new CartDto(cart.Id, cart.Status, responseItems);
    }

    private sealed record CartCatalogRow(Guid VariantId, Guid ProductId, string ProductName, string Slug,
        ProductStatus ProductStatus, string Sku, string VariantName, VariantStatus VariantStatus, string? UnitLabel);
}
