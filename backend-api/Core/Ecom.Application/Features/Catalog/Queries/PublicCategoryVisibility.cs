using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Catalog.Queries.GetPublicCategories;

internal static class PublicCategoryVisibility
{
    public static Task<List<CategoryRow>> LoadAsync(IUnitOfWork unitOfWork, CancellationToken cancellationToken) =>
        unitOfWork.Repository<Category>().QueryNoTracking()
            .OrderBy(category => category.DisplayOrder).ThenBy(category => category.Name)
            .Select(category => new CategoryRow(
                category.Id,
                category.ParentId,
                category.Name,
                category.Slug,
                category.Description,
                category.DisplayOrder,
                category.Status))
            .ToListAsync(cancellationToken);

    public static bool HasPublishedAncestors(CategoryRow category, IReadOnlyDictionary<Guid, CategoryRow> byId)
    {
        var visited = new HashSet<Guid>();
        var parentId = category.ParentId;
        while (parentId.HasValue)
        {
            if (!visited.Add(parentId.Value) || !byId.TryGetValue(parentId.Value, out var parent))
                return false;
            if (parent.Status != CatalogStatus.Published)
                return false;
            parentId = parent.ParentId;
        }
        return true;
    }

    internal sealed record CategoryRow(
        Guid Id,
        Guid? ParentId,
        string Name,
        string Slug,
        string? Description,
        int DisplayOrder,
        CatalogStatus Status);
}
