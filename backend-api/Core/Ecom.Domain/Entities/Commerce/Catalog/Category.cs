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

    public static Category Create(Guid? parentId, string name, string slug, string? description, int displayOrder)
    {
        var category = new Category();
        category.UpdateDetails(parentId, name, slug, description, displayOrder);
        category.Status = CatalogStatus.Draft;
        return category;
    }

    public void UpdateDetails(Guid? parentId, string name, string slug, string? description, int displayOrder)
    {
        if (Status == CatalogStatus.Hidden)
            throw new CommerceDomainException("CATEGORY_HIDDEN", "A hidden category cannot be changed.");

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(slug))
            throw new CommerceDomainException("CATEGORY_DETAILS_INVALID", "Category name and slug are required.");
        if (displayOrder < 0)
            throw new CommerceDomainException("CATEGORY_DISPLAY_ORDER_INVALID", "Category display order cannot be negative.");

        ParentId = parentId;
        Name = name.Trim();
        Slug = slug.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        DisplayOrder = displayOrder;
    }

    public void Publish()
    {
        if (Status == CatalogStatus.Hidden)
            throw new CommerceDomainException("CATEGORY_HIDDEN", "A hidden category cannot be published.");
        Status = CatalogStatus.Published;
    }

    public void Pause()
    {
        if (Status == CatalogStatus.Hidden)
            throw new CommerceDomainException("CATEGORY_HIDDEN", "A hidden category cannot be paused.");
        if (Status != CatalogStatus.Published)
            throw new CommerceDomainException("CATEGORY_NOT_PUBLISHED", "Only a published category can be paused.");
        Status = CatalogStatus.Paused;
    }

    public void Hide()
    {
        if (Status == CatalogStatus.Hidden)
            throw new CommerceDomainException("CATEGORY_HIDDEN", "The category is already hidden.");
        Status = CatalogStatus.Hidden;
    }

    public Guid RenewConcurrencyStamp()
    {
        ConcurrencyStamp = Guid.NewGuid();
        return ConcurrencyStamp;
    }
}
