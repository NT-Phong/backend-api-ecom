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
}