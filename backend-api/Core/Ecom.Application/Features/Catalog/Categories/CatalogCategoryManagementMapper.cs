using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Catalog.Categories;

internal static class CatalogCategoryManagementMapper
{
    public static CatalogCategoryManagementDto Map(
        Category category,
        CatalogCategoryParentDto? parent = null,
        int childrenCount = 0,
        int productCount = 0,
        int publishedProductCount = 0) =>
        new(category.Id, category.ParentId, category.Name, category.Slug, category.Description, category.DisplayOrder,
            category.Status, category.ConcurrencyStamp, parent, childrenCount, productCount, publishedProductCount,
            category.CreatedAt, category.UpdatedAt);
}
