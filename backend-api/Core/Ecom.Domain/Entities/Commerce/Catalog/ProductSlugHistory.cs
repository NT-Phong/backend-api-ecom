namespace Ecom.Domain.Entities;
public class ProductSlugHistory : BaseEntity
{
    public Guid ProductId { get; private set; }
    public string Slug { get; private set; } = string.Empty;
    public int RedirectStatusCode { get; private set; } = 301;

    private ProductSlugHistory()
    {
    }
}