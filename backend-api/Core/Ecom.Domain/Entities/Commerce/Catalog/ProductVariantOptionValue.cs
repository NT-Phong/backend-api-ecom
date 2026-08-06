namespace Ecom.Domain.Entities;
public class ProductVariantOptionValue : BaseEntity
{
    public Guid ProductVariantId { get; private set; }
    public Guid ProductOptionValueId { get; private set; }

    private ProductVariantOptionValue()
    {
    }

    internal static ProductVariantOptionValue Create(Guid productVariantId, Guid productOptionValueId)
    {
        if (productVariantId == Guid.Empty || productOptionValueId == Guid.Empty)
            throw new CommerceDomainException("VARIANT_OPTION_VALUE_INVALID", "Variant and option value are required.");
        return new ProductVariantOptionValue { ProductVariantId = productVariantId, ProductOptionValueId = productOptionValueId };
    }
}
