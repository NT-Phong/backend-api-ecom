namespace Ecom.Domain.Entities;
public class Page : BaseEntity
{
    public string Title { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public ContentStatus Status { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public string? MetaTitle { get; private set; }
    public string? MetaDescription { get; private set; }

    private Page()
    {
    }
}