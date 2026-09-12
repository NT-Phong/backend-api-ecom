using Ecom.Application.Features.Catalog.Common;
using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Catalog.Products.Services;

public sealed class CatalogReadinessService(IUnitOfWork unitOfWork,
    IEffectivePriceResolver effectivePriceResolver,
    IProductAvailabilityReadService availabilityReadService) : ICatalogReadinessService
{
    public async Task<CatalogProductReadinessDto?> GetAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var product = await unitOfWork.Repository<Product>().QueryNoTracking()
            .SingleOrDefaultAsync(x => x.Id == productId, cancellationToken);
        if (product is null)
            return null;

        var producerPublicAndVerified = await unitOfWork.Repository<Producer>().QueryNoTracking()
            .AnyAsync(x => x.Id == product.ProducerId && x.PublicStatus == PublicStatus.Published && x.IsVerified,
                cancellationToken);
        var primaryCategoryPublic = await (
            from map in unitOfWork.Repository<ProductCategory>().QueryNoTracking()
            join category in unitOfWork.Repository<Category>().QueryNoTracking() on map.CategoryId equals category.Id
            where map.ProductId == product.Id && map.IsPrimary && category.Status == CatalogStatus.Published
            select map.Id).AnyAsync(cancellationToken);
        var primaryMediaPublic = await (
            from map in unitOfWork.Repository<ProductMedia>().QueryNoTracking()
            join asset in unitOfWork.Repository<MediaAsset>().QueryNoTracking() on map.MediaAssetId equals asset.Id
            where map.ProductId == product.Id && map.IsPrimary && asset.Visibility == MediaVisibility.Public
                && asset.ScanStatus == MediaScanStatus.Clean
            select map.Id).AnyAsync(cancellationToken);
        var activeVariants = await unitOfWork.Repository<ProductVariant>().QueryNoTracking()
            .Where(x => x.ProductId == product.Id && x.Status == VariantStatus.Active)
            .Select(x => new { x.Id, x.InventoryMode })
            .ToListAsync(cancellationToken);
        var effectivePrices = await effectivePriceResolver.ResolveForVariantsAsync(activeVariants.Select(x => x.Id).ToArray(),
            DateTime.UtcNow, cancellationToken);
        var trackedIds = activeVariants.Where(x => x.InventoryMode == InventoryMode.Tracked).Select(x => x.Id).ToArray();
        var initializedTrackedIds = trackedIds.Length == 0
            ? new HashSet<Guid>()
            : (await (
                from item in unitOfWork.Repository<InventoryItem>().QueryNoTracking()
                join level in unitOfWork.Repository<InventoryLevel>().QueryNoTracking() on item.Id equals level.InventoryItemId
                join location in unitOfWork.Repository<StockLocation>().QueryNoTracking() on level.StockLocationId equals location.Id
                where trackedIds.Contains(item.ProductVariantId) && location.Code == "MAIN" && location.IsActive
                select item.ProductVariantId).Distinct().ToListAsync(cancellationToken)).ToHashSet();
        var trackedInitialized = trackedIds.All(initializedTrackedIds.Contains);
        var availability = await availabilityReadService.ResolveForProductsAsync([product.Id], DateTime.UtcNow,
            cancellationToken);
        var canPublish = producerPublicAndVerified && primaryCategoryPublic && primaryMediaPublic
            && activeVariants.Count > 0 && effectivePrices.Count > 0;
        var canSell = availability.GetValueOrDefault(product.Id) == CatalogAvailabilityStatus.Available;

        return new CatalogProductReadinessDto(product.Id, canPublish, canSell,
        [
            new("PRODUCER_NOT_PUBLIC_OR_VERIFIED", producerPublicAndVerified),
            new("PRIMARY_CATEGORY_MISSING_OR_NOT_PUBLIC", primaryCategoryPublic),
            new("PRIMARY_MEDIA_MISSING_OR_NOT_PUBLIC", primaryMediaPublic),
            new("ACTIVE_VARIANT_MISSING", activeVariants.Count > 0),
            new("EFFECTIVE_PRICE_MISSING", effectivePrices.Count > 0),
            new("TRACKED_VARIANT_MAIN_LEVEL_MISSING", trackedInitialized)
        ]);
    }
}
