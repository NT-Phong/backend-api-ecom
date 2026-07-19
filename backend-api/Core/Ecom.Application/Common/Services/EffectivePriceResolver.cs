using Ecom.Domain.Entities;

namespace Ecom.Application.Common.Services;

public sealed class EffectivePriceResolver(IUnitOfWork unitOfWork) : IEffectivePriceResolver
{
    public async Task<IReadOnlyDictionary<Guid, EffectiveVariantPrice>> ResolveForVariantsAsync(
        IReadOnlyCollection<Guid> variantIds, DateTime asOfUtc, CancellationToken cancellationToken = default)
    {
        if (variantIds.Count == 0) return new Dictionary<Guid, EffectiveVariantPrice>();

        var ids = variantIds.Distinct().ToArray();
        var candidates = await QueryEffectivePriceCandidates(asOfUtc)
            .Where(x => ids.Contains(x.ProductVariantId))
            .ToListAsync(cancellationToken);

        return candidates
            .GroupBy(x => x.ProductVariantId)
            .Select(group => group
                .OrderBy(x => x.PriceType == PriceType.Sale ? 0 : 1)
                .ThenByDescending(x => x.EffectiveFrom)
                .ThenBy(x => x.VariantPriceId)
                .First())
            .ToDictionary(x => x.ProductVariantId);
    }

    public IQueryable<EffectiveVariantPrice> QueryEffectivePriceCandidates(DateTime asOfUtc)
    {
        var prices = unitOfWork.Repository<VariantPrice>().QueryNoTracking();
        var priceLists = unitOfWork.Repository<PriceList>().QueryNoTracking();

        return
            from price in prices
            join priceList in priceLists on price.PriceListId equals priceList.Id into priceListGroup
            from priceList in priceListGroup.DefaultIfEmpty()
            where price.CurrencyCode == CommerceConstants.DefaultCurrency
                  && price.MinQuantity == 1
                  && (price.PriceType == PriceType.Sale || price.PriceType == PriceType.Public)
                  && price.EffectiveFrom <= asOfUtc
                  && (price.EffectiveTo == null || price.EffectiveTo > asOfUtc)
                  && (price.PriceListId == null ||
                      (priceList != null && priceList.Status == PriceListStatus.Active
                       && (priceList.StartsAt == null || priceList.StartsAt <= asOfUtc)
                       && (priceList.EndsAt == null || priceList.EndsAt > asOfUtc)))
            select new EffectiveVariantPrice(price.Id, price.ProductVariantId, price.Amount, price.CurrencyCode,
                price.PriceType, price.EffectiveFrom);
    }
}
