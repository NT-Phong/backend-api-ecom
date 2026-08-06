namespace Ecom.Application.Common.Interfaces;

public interface IEffectivePriceResolver
{
    IQueryable<EffectiveVariantPrice> QueryEffectivePriceCandidates(DateTime asOfUtc);
    IQueryable<EffectiveProductPrice> QueryEffectiveProductPrices(DateTime asOfUtc);

    Task<IReadOnlyDictionary<Guid, EffectiveVariantPrice>> ResolveForVariantsAsync(
        IReadOnlyCollection<Guid> variantIds, DateTime asOfUtc, CancellationToken cancellationToken = default);
}

public sealed record EffectiveProductPrice(Guid ProductId, decimal Amount, string CurrencyCode);
