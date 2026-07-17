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

    private VariantPrice()
    {
    }
}