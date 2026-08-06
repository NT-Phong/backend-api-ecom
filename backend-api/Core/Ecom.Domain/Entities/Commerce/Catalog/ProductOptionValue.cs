namespace Ecom.Domain.Entities;
public class ProductOptionValue : BaseEntity
{
    public Guid ProductOptionId { get; private set; }
    public string Value { get; private set; } = string.Empty;
    public int DisplayOrder { get; private set; }

    private ProductOptionValue()
    {
    }

    internal static ProductOptionValue Create(Guid productOptionId, string value, int displayOrder)
    {
        if (productOptionId == Guid.Empty || string.IsNullOrWhiteSpace(value) || displayOrder < 0)
            throw new CommerceDomainException("PRODUCT_OPTION_VALUE_INVALID", "Product option value details are invalid.");
        return new ProductOptionValue { ProductOptionId = productOptionId, Value = value.Trim(), DisplayOrder = displayOrder };
    }

    internal void Update(string value, int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(value) || displayOrder < 0)
            throw new CommerceDomainException("PRODUCT_OPTION_VALUE_INVALID", "Product option value details are invalid.");
        Value = value.Trim(); DisplayOrder = displayOrder;
    }
}
