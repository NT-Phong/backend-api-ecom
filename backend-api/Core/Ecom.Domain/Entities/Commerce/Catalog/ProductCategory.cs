namespace Ecom.Domain.Entities;
public class ProductCategory : BaseEntity
{
    public Guid ProductId { get; private set; }
    public Guid CategoryId { get; private set; }
    public bool IsPrimary { get; private set; }

    internal static ProductCategory Create(Guid productId, Guid categoryId, bool isPrimary)
    {
        if (productId == Guid.Empty || categoryId == Guid.Empty)
            throw new CommerceDomainException("PRODUCT_CATEGORY_REFERENCE_REQUIRED", "Product and category are required.");

        return new ProductCategory
        {
            ProductId = productId,
            CategoryId = categoryId,
            IsPrimary = isPrimary
        };
    }

    private ProductCategory()
    {
    }
}
