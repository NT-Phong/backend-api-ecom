namespace Ecom.Domain.Entities;
public class Category : BaseEntity
{
    public Guid? ParentId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int DisplayOrder { get; private set; }
    public CatalogStatus Status { get; private set; }

    private Category()
    {
    }
}