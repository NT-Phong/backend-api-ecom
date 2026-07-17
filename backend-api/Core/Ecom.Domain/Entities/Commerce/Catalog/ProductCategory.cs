namespace Ecom.Domain.Entities;
public class ProductCategory : BaseEntity
{
    public Guid ProductId { get; private set; }
    public Guid CategoryId { get; private set; }
    public bool IsPrimary { get; private set; }

    private ProductCategory()
    {
    }
}