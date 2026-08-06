namespace Ecom.Application.Features.Catalog.Categories;

public sealed record CatalogCategoryManagementDto(Guid Id, Guid? ParentId, string Name, string Slug, string? Description,
    int DisplayOrder, CatalogStatus Status, Guid ConcurrencyStamp, CatalogCategoryParentDto? Parent = null,
    int ChildrenCount = 0, int ProductCount = 0, int PublishedProductCount = 0,
    DateTime? CreatedAt = null, DateTime? UpdatedAt = null);

public sealed record CatalogCategoryParentDto(Guid Id, string Name, string Slug);
