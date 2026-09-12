using Ecom.Application.Features.Catalog.Common;
using Ecom.Domain.Entities;

namespace Ecom.Application.Common.Services;

/// <summary>
/// Resolves public availability in bounded queries. Quantity never leaves this service.
/// </summary>
public sealed class ProductAvailabilityReadService(IUnitOfWork unitOfWork,
    IEffectivePriceResolver effectivePriceResolver) : IProductAvailabilityReadService
{
    public async Task<IReadOnlyDictionary<Guid, CatalogAvailabilityStatus>> ResolveForVariantsAsync(
        IReadOnlyCollection<Guid> variantIds, DateTime asOfUtc, CancellationToken cancellationToken = default)
    {
        if (variantIds.Count == 0)
            return new Dictionary<Guid, CatalogAvailabilityStatus>();

        var ids = variantIds.Distinct().ToArray();
        var variants = await unitOfWork.Repository<ProductVariant>().QueryNoTracking()
            .Where(x => ids.Contains(x.Id))
            .Select(x => new VariantFact(x.Id, x.InventoryMode, x.Status))
            .ToListAsync(cancellationToken);
        var prices = await effectivePriceResolver.ResolveForVariantsAsync(variants.Select(x => x.Id).ToArray(),
            asOfUtc, cancellationToken);

        var trackedIds = variants.Where(x => x.InventoryMode == InventoryMode.Tracked)
            .Select(x => x.Id).ToArray();
        var mainAvailability = trackedIds.Length == 0
            ? new Dictionary<Guid, decimal>()
            : await (
                from item in unitOfWork.Repository<InventoryItem>().QueryNoTracking()
                join level in unitOfWork.Repository<InventoryLevel>().QueryNoTracking()
                    on item.Id equals level.InventoryItemId
                join location in unitOfWork.Repository<StockLocation>().QueryNoTracking()
                    on level.StockLocationId equals location.Id
                where trackedIds.Contains(item.ProductVariantId)
                      && location.Code == "MAIN"
                      && location.IsActive
                group level by item.ProductVariantId into grouped
                select new { VariantId = grouped.Key, Available = grouped.Sum(x => x.StockedQuantity - x.ReservedQuantity) })
                .ToDictionaryAsync(x => x.VariantId, x => x.Available, cancellationToken);

        return variants.ToDictionary(x => x.Id, x =>
        {
            if (x.Status != VariantStatus.Active || !prices.ContainsKey(x.Id))
                return CatalogAvailabilityStatus.Unavailable;
            if (x.InventoryMode == InventoryMode.NotTracked)
                return CatalogAvailabilityStatus.Available;
            if (x.InventoryMode != InventoryMode.Tracked)
                return CatalogAvailabilityStatus.Unavailable;
            return mainAvailability.TryGetValue(x.Id, out var available) && available > 0
                ? CatalogAvailabilityStatus.Available
                : CatalogAvailabilityStatus.OutOfStock;
        });
    }

    public async Task<IReadOnlyDictionary<Guid, CatalogAvailabilityStatus>> ResolveForProductsAsync(
        IReadOnlyCollection<Guid> productIds, DateTime asOfUtc, CancellationToken cancellationToken = default)
    {
        if (productIds.Count == 0)
            return new Dictionary<Guid, CatalogAvailabilityStatus>();

        var ids = productIds.Distinct().ToArray();
        var variants = await unitOfWork.Repository<ProductVariant>().QueryNoTracking()
            .Where(x => ids.Contains(x.ProductId))
            .Select(x => new { x.Id, x.ProductId })
            .ToListAsync(cancellationToken);
        var availability = await ResolveForVariantsAsync(variants.Select(x => x.Id).ToArray(), asOfUtc,
            cancellationToken);
        return ids.ToDictionary(productId => productId,
            productId => SummarizeProduct(variants.Where(x => x.ProductId == productId)
                .Select(x => availability[x.Id])));
    }

    public CatalogAvailabilityStatus SummarizeProduct(IEnumerable<CatalogAvailabilityStatus> variantAvailability)
    {
        var values = variantAvailability as CatalogAvailabilityStatus[] ?? variantAvailability.ToArray();
        if (values.Contains(CatalogAvailabilityStatus.Available))
            return CatalogAvailabilityStatus.Available;
        return values.Contains(CatalogAvailabilityStatus.OutOfStock)
            ? CatalogAvailabilityStatus.OutOfStock
            : CatalogAvailabilityStatus.Unavailable;
    }

    private sealed record VariantFact(Guid Id, InventoryMode InventoryMode, VariantStatus Status);
}
