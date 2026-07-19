namespace Ecom.Application.Common.Interfaces;

public interface IEffectivePriceResolver
{
    IQueryable<EffectiveVariantPrice> QueryEffectivePriceCandidates(DateTime asOfUtc);

    Task<IReadOnlyDictionary<Guid, EffectiveVariantPrice>> ResolveForVariantsAsync(
        IReadOnlyCollection<Guid> variantIds, DateTime asOfUtc, CancellationToken cancellationToken = default);
}
