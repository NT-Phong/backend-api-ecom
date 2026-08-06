using Ecom.Domain.Entities;

namespace Ecom.Application.Common.Services;

public sealed class EffectivePriceResolver(IUnitOfWork unitOfWork) : IEffectivePriceResolver
{
    public async Task<IReadOnlyDictionary<Guid, EffectiveVariantPrice>> ResolveForVariantsAsync(
        IReadOnlyCollection<Guid> variantIds, DateTime asOfUtc, CancellationToken cancellationToken = default)
    {
        if (variantIds.Count == 0) return new Dictionary<Guid, EffectiveVariantPrice>();

        var ids = variantIds.Distinct().ToArray();
        var prices = unitOfWork.Repository<VariantPrice>().QueryNoTracking();
        var priceLists = unitOfWork.Repository<PriceList>().QueryNoTracking();

        // Apply the local ID collection while this is still an entity query. Composing
        // Contains over the EffectiveVariantPrice record projection is not translated by
        // Npgsql in the publish command path.
        var candidateRows = await (
                from price in prices
                join priceList in priceLists on price.PriceListId equals priceList.Id into priceListGroup
                from priceList in priceListGroup.DefaultIfEmpty()
                where ids.Contains(price.ProductVariantId)
                      && price.CurrencyCode == CommerceConstants.DefaultCurrency
                      && price.MinQuantity == 1
                      && (price.PriceType == PriceType.Sale || price.PriceType == PriceType.Public)
                      && price.EffectiveFrom <= asOfUtc
                      && (price.EffectiveTo == null || price.EffectiveTo > asOfUtc)
                      && (price.PriceListId == null ||
                          (priceList != null && priceList.Status == PriceListStatus.Active
                           && (priceList.StartsAt == null || priceList.StartsAt <= asOfUtc)
                           && (priceList.EndsAt == null || priceList.EndsAt > asOfUtc)))
                select new
                {
                    price.Id,
                    price.ProductVariantId,
                    price.Amount,
                    price.CurrencyCode,
                    price.PriceType,
                    price.EffectiveFrom
                })
            .ToListAsync(cancellationToken);

        var candidates = candidateRows.Select(x => new EffectiveVariantPrice(
            x.Id,
            x.ProductVariantId,
            x.Amount,
            x.CurrencyCode,
            x.PriceType,
            x.EffectiveFrom));

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

        var prices = unitOfWork.Repository<VariantPrice>().QueryNoTracking();
        var priceLists = unitOfWork.Repository<PriceList>().QueryNoTracking();

        // Keep the composable part of this query entity/anonymous-projection based.
        // Catalog management composes EXISTS filters over it; Npgsql can translate
        // those reliably, unlike a filter composed over EffectiveVariantPrice.
        var candidates =
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
            select new
            {
                price.Id,
                price.ProductVariantId,
                price.Amount,
                price.PriceType,
                price.EffectiveFrom
            };

        // A NOT EXISTS anti-join chooses the effective price per active variant:
        // Sale before Public, then latest effective date. The VariantPrice exclusion
        // constraint prevents same-tier overlapping candidates.
        var effectiveVariantPrices =
            from candidate in candidates
            where !candidates.Any(other =>
                other.ProductVariantId == candidate.ProductVariantId &&
                ((other.PriceType == PriceType.Sale && candidate.PriceType == PriceType.Public) ||
                 (other.PriceType == candidate.PriceType && other.EffectiveFrom > candidate.EffectiveFrom)))
            join variant in activeVariants on candidate.ProductVariantId equals variant.Id
            select new { variant.ProductId, candidate.Amount };

        return from price in effectiveVariantPrices
               group price.Amount by price.ProductId into productPrices
               select new EffectiveProductPrice(productPrices.Key, productPrices.Min(), CommerceConstants.DefaultCurrency);
    }
}
