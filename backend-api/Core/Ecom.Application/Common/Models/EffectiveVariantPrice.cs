namespace Ecom.Application.Common.Models;

public sealed record EffectiveVariantPrice(Guid VariantPriceId, Guid ProductVariantId, decimal Amount, string CurrencyCode,
    PriceType PriceType, DateTime EffectiveFrom);
