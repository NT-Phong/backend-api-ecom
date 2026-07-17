namespace Ecom.Domain.Entities;
public class ArticleCategory : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    private ArticleCategory()
    {
    }
}