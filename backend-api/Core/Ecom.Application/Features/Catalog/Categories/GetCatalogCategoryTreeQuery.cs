using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Catalog.Categories;

public sealed record GetCatalogCategoryTreeQuery : IRequest<TResult<IReadOnlyList<CatalogCategoryTreeItemDto>>>;

public sealed record CatalogCategoryTreeItemDto(Guid Id, string Name, string Slug, CatalogStatus Status,
    int DisplayOrder, IReadOnlyList<CatalogCategoryTreeItemDto> Children);

public sealed class GetCatalogCategoryTreeQueryHandler(IUnitOfWork unitOfWork, ICatalogProductAccessService access)
    : IRequestHandler<GetCatalogCategoryTreeQuery, TResult<IReadOnlyList<CatalogCategoryTreeItemDto>>>
{
    public async Task<TResult<IReadOnlyList<CatalogCategoryTreeItemDto>>> Handle(GetCatalogCategoryTreeQuery request, CancellationToken cancellationToken)
    {
        var authorization = access.Ensure(Permissions.CatalogCategories.Read);
        if (!authorization.IsSuccess)
            return TResult<IReadOnlyList<CatalogCategoryTreeItemDto>>.Failure(authorization.Error!, authorization.ErrorCode);

        var categories = await unitOfWork.Repository<Category>().QueryNoTracking()
            .Select(x => new CategoryNode(x.Id, x.ParentId, x.Name, x.Slug, x.Status, x.DisplayOrder))
            .OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name).ToListAsync(cancellationToken);
        var childrenByParent = categories.Where(x => x.ParentId.HasValue).GroupBy(x => x.ParentId!.Value)
            .ToDictionary(x => x.Key, x => x.ToList());
        var roots = categories.Where(x => !x.ParentId.HasValue || !categories.Any(parent => parent.Id == x.ParentId.Value));
        return TResult<IReadOnlyList<CatalogCategoryTreeItemDto>>.Success(roots.Select(x => Map(x, childrenByParent, new HashSet<Guid>())).ToList());
    }

    private static CatalogCategoryTreeItemDto Map(CategoryNode node, IReadOnlyDictionary<Guid, List<CategoryNode>> childrenByParent, HashSet<Guid> path)
    {
        if (!path.Add(node.Id))
            throw new CommerceDomainException("CATEGORY_PARENT_CYCLE", "The category hierarchy contains a cycle.");
        var children = childrenByParent.TryGetValue(node.Id, out var descendants)
            ? descendants.Select(x => Map(x, childrenByParent, new HashSet<Guid>(path))).ToList()
            : [];
        return new CatalogCategoryTreeItemDto(node.Id, node.Name, node.Slug, node.Status, node.DisplayOrder, children);
    }

    private sealed record CategoryNode(Guid Id, Guid? ParentId, string Name, string Slug, CatalogStatus Status, int DisplayOrder);
}
