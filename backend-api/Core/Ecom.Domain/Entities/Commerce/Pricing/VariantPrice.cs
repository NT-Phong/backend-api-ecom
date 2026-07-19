namespace Ecom.Domain.Entities;
public class VariantPrice : BaseEntity
{
    public Guid ProductVariantId { get; private set; }
    public Guid? PriceListId { get; private set; }
    public string CurrencyCode { get; private set; } = "VND";
    public decimal Amount { get; private set; }
    public int MinQuantity { get; private set; } = 1;
    public PriceType PriceType { get; private set; }
    public DateTime EffectiveFrom { get; private set; }
    public DateTime? EffectiveTo { get; private set; }

    public static VariantPrice Create(Guid productVariantId, decimal amount, PriceType priceType,
        DateTime effectiveFrom, DateTime? effectiveTo = null, Guid? priceListId = null,
        string currencyCode = "VND", int minQuantity = 1)
    {
        if (productVariantId == Guid.Empty)
            throw new CommerceDomainException("VARIANT_PRICE_VARIANT_REQUIRED", "A product variant is required.");
        if (amount < 0)
            throw new CommerceDomainException("VARIANT_PRICE_AMOUNT_INVALID", "Price amount cannot be negative.");
        if (string.IsNullOrWhiteSpace(currencyCode) || currencyCode.Trim().Length != 3)
            throw new CommerceDomainException("VARIANT_PRICE_CURRENCY_INVALID", "Currency code must contain three characters.");
        if (minQuantity < 1)
            throw new CommerceDomainException("VARIANT_PRICE_MINIMUM_QUANTITY_INVALID", "Minimum quantity must be at least one.");
        if (effectiveFrom == default || effectiveTo is not null && effectiveTo <= effectiveFrom)
            throw new CommerceDomainException("VARIANT_PRICE_TIME_WINDOW_INVALID", "Price effective window is invalid.");

        return new VariantPrice
        {
            ProductVariantId = productVariantId,
            PriceListId = priceListId,
            CurrencyCode = currencyCode.Trim().ToUpperInvariant(),
            Amount = amount,
            MinQuantity = minQuantity,
            PriceType = priceType,
            EffectiveFrom = effectiveFrom,
            EffectiveTo = effectiveTo
        };
    }

    private VariantPrice()
    {
    }
}
