namespace Ecom.Domain.Entities;
public class ProductOption : BaseEntity
{
    public Guid ProductId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public int DisplayOrder { get; private set; }

    private ProductOption()
    {
    }

    internal static ProductOption Create(Guid productId, string code, string name, int displayOrder)
    {
        if (productId == Guid.Empty || string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name) || displayOrder < 0)
            throw new CommerceDomainException("PRODUCT_OPTION_INVALID", "Product option details are invalid.");
        return new ProductOption { ProductId = productId, Code = code.Trim(), Name = name.Trim(), DisplayOrder = displayOrder };
    }

    internal void Update(string name, int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(name) || displayOrder < 0)
            throw new CommerceDomainException("PRODUCT_OPTION_INVALID", "Product option details are invalid.");
        Name = name.Trim(); DisplayOrder = displayOrder;
    }
}
