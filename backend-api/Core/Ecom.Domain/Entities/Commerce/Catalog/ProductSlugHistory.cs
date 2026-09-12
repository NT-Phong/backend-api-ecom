namespace Ecom.Domain.Entities;
public class ProductSlugHistory : BaseEntity
{
    public Guid ProductId { get; private set; }
    public string Slug { get; private set; } = string.Empty;
    public int RedirectStatusCode { get; private set; } = 301;

    private ProductSlugHistory()
    {
    }

    public static ProductSlugHistory Create(Guid productId, string slug)
    {
        if (productId == Guid.Empty)
            throw new CommerceDomainException("PRODUCT_SLUG_HISTORY_PRODUCT_REQUIRED", "A product is required.");
        if (string.IsNullOrWhiteSpace(slug))
            throw new CommerceDomainException("PRODUCT_SLUG_HISTORY_SLUG_REQUIRED", "A slug is required.");

        return new ProductSlugHistory { ProductId = productId, Slug = slug.Trim(), RedirectStatusCode = 301 };
    }
}
