namespace Ecom.Domain.Entities;
public class ArticleCategoryMap : BaseEntity
{
    public Guid ArticleId { get; private set; }
    public Guid ArticleCategoryId { get; private set; }

    private ArticleCategoryMap()
    {
    }
}