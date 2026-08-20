namespace Ecom.Application.Common.Interfaces;

public interface IEffectivePriceResolver
{
    IQueryable<EffectiveVariantPrice> QueryEffectivePriceCandidates(DateTime asOfUtc);
    IQueryable<EffectiveProductPrice> QueryEffectiveProductPrices(DateTime asOfUtc,
        IReadOnlyCollection<Guid>? productIds = null);
    IQueryable<Guid> QueryProductIdsWithEffectivePrice(DateTime asOfUtc, decimal? minimumAmount = null,
        decimal? maximumAmount = null);

    Task<IReadOnlyDictionary<Guid, EffectiveVariantPrice>> ResolveForVariantsAsync(
        IReadOnlyCollection<Guid> variantIds, DateTime asOfUtc, CancellationToken cancellationToken = default);
}

public sealed record EffectiveProductPrice(Guid ProductId, decimal Amount, string CurrencyCode);
