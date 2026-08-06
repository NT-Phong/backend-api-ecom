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

    public IQueryable<EffectiveProductPrice> QueryEffectiveProductPrices(DateTime asOfUtc)
    {
        var activeVariants = unitOfWork.Repository<ProductVariant>().QueryNoTracking()
            .Where(x => x.Status == VariantStatus.Active)
            .Select(x => new { x.Id, x.ProductId });

        var effectiveVariantPrices =
            from candidate in QueryEffectivePriceCandidates(asOfUtc)
            join variant in activeVariants on candidate.ProductVariantId equals variant.Id
            group new { candidate, variant.ProductId } by new { variant.ProductId, candidate.ProductVariantId } into prices
            select new { prices.Key.ProductId, Price = prices.OrderBy(x => x.candidate.PriceType == PriceType.Sale ? 0 : 1)
                .ThenByDescending(x => x.candidate.EffectiveFrom)
                .ThenBy(x => x.candidate.VariantPriceId)
                .Select(x => x.candidate.Amount)
                .First() };

        return from price in effectiveVariantPrices
               group price by price.ProductId into prices
               select new EffectiveProductPrice(prices.Key, prices.Min(x => x.Price), CommerceConstants.DefaultCurrency);
    }
}
