namespace Ecom.Domain.Entities;
public class ProductOptionValue : BaseEntity
{
    public Guid ProductOptionId { get; private set; }
    public string Value { get; private set; } = string.Empty;
    public int DisplayOrder { get; private set; }

    private ProductOptionValue()
    {
    }
}