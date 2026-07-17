namespace Ecom.Domain.Entities;
public class PageSection : BaseEntity
{
    public Guid PageId { get; private set; }
    public string SectionType { get; private set; } = string.Empty;
    public string? Title { get; private set; }
    public string? Content { get; private set; }
    public int DisplayOrder { get; private set; }
    public ContentStatus Status { get; private set; }

    private PageSection()
    {
    }
}