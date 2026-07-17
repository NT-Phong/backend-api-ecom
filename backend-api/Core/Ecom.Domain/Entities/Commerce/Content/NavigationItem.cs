namespace Ecom.Domain.Entities;
public class NavigationItem : BaseEntity
{
    public Guid? ParentId { get; private set; }
    public Guid? PageId { get; private set; }
    public string Label { get; private set; } = string.Empty;
    public string? TargetUrl { get; private set; }
    public int DisplayOrder { get; private set; }
    public ContentStatus Status { get; private set; }

    private NavigationItem()
    {
    }
}