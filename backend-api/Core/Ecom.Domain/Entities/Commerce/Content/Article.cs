namespace Ecom.Domain.Entities;
public class Article : BaseEntity
{
    public Guid? AuthorUserId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? Summary { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public Guid? CoverMediaAssetId { get; private set; }
    public ContentStatus Status { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public string? MetaTitle { get; private set; }
    public string? MetaDescription { get; private set; }

    private Article()
    {
    }
}