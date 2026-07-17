namespace Ecom.Domain.Entities;
public class ProductVariantOptionValue : BaseEntity
{
    public Guid ProductVariantId { get; private set; }
    public Guid ProductOptionValueId { get; private set; }

    private ProductVariantOptionValue()
    {
    }
}