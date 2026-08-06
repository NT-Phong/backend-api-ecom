using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Catalog.Categories;

public sealed class GetCatalogCategoryListQuery : IRequest<TResult<PaginatedList<CatalogCategoryManagementDto>>>
{
    public string? Q { get; init; }
    public CatalogStatus? Status { get; init; }
    public Guid? ParentId { get; init; }
    public bool? HasChildren { get; init; }
    public bool? HasProducts { get; init; }
    public bool? HasPublishedProducts { get; init; }
    public string Sort { get; init; } = "displayOrder";
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
    public int Skip() => (Page - 1) * PageSize;
}

public sealed class GetCatalogCategoryListQueryValidator : AbstractValidator<GetCatalogCategoryListQuery>
{
    private static readonly string[] SupportedSorts = ["displayOrder", "name", "createdAt", "updatedAt"];

    public GetCatalogCategoryListQueryValidator()
    {
        RuleFor(x => x.Q).MaximumLength(250);
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Sort).Must(x => SupportedSorts.Contains(x, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Sort is not supported.");
    }
}

public sealed class GetCatalogCategoryListQueryHandler(IUnitOfWork unitOfWork, ICatalogProductAccessService access)
    : IRequestHandler<GetCatalogCategoryListQuery, TResult<PaginatedList<CatalogCategoryManagementDto>>>
{
    public async Task<TResult<PaginatedList<CatalogCategoryManagementDto>>> Handle(
        GetCatalogCategoryListQuery request,
        CancellationToken cancellationToken)
    {
        var authorization = access.Ensure(Permissions.CatalogCategories.Read);
        if (!authorization.IsSuccess)
            return TResult<PaginatedList<CatalogCategoryManagementDto>>.Failure(authorization.Error!, authorization.ErrorCode);

        var categories = unitOfWork.Repository<Category>().QueryNoTracking();
        var mappings = unitOfWork.Repository<ProductCategory>().QueryNoTracking();
        var products = unitOfWork.Repository<Product>().QueryNoTracking();
        if (!string.IsNullOrWhiteSpace(request.Q))
        {
            var query = request.Q.Trim().ToLower();
            categories = categories.Where(x => x.Name.ToLower().Contains(query) || x.Slug.ToLower().Contains(query));
        }
        if (request.Status.HasValue) categories = categories.Where(x => x.Status == request.Status.Value);
        if (request.ParentId.HasValue) categories = categories.Where(x => x.ParentId == request.ParentId.Value);
        if (request.HasChildren.HasValue)
            categories = request.HasChildren.Value
                ? categories.Where(x => unitOfWork.Repository<Category>().QueryNoTracking().Any(child => child.ParentId == x.Id))
                : categories.Where(x => !unitOfWork.Repository<Category>().QueryNoTracking().Any(child => child.ParentId == x.Id));
        if (request.HasProducts.HasValue)
            categories = request.HasProducts.Value
                ? categories.Where(x => mappings.Any(mapping => mapping.CategoryId == x.Id))
                : categories.Where(x => !mappings.Any(mapping => mapping.CategoryId == x.Id));
        if (request.HasPublishedProducts.HasValue)
            categories = request.HasPublishedProducts.Value
                ? categories.Where(category => mappings.Join(products, mapping => mapping.ProductId, product => product.Id, (mapping, product) => new { mapping, product })
                    .Any(row => row.mapping.CategoryId == category.Id && row.product.Status == ProductStatus.Published))
                : categories.Where(category => !mappings.Join(products, mapping => mapping.ProductId, product => product.Id, (mapping, product) => new { mapping, product })
                    .Any(row => row.mapping.CategoryId == category.Id && row.product.Status == ProductStatus.Published));

        var total = await categories.CountAsync(cancellationToken);
        var ordered = request.Sort.ToLowerInvariant() switch
        {
            "name" => categories.OrderBy(x => x.Name).ThenBy(x => x.Id),
            "createdat" => categories.OrderByDescending(x => x.CreatedAt).ThenBy(x => x.Id),
            "updatedat" => categories.OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt).ThenBy(x => x.Id),
            _ => categories.OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name).ThenBy(x => x.Id)
        };
        var page = await ordered.Skip(request.Skip()).Take(request.PageSize).ToListAsync(cancellationToken);
        var pageIds = page.Select(x => x.Id).ToArray();
        var allCategories = await unitOfWork.Repository<Category>().QueryNoTracking()
            .Select(x => new { x.Id, x.Name, x.Slug, x.ParentId }).ToListAsync(cancellationToken);
        var parents = allCategories.ToDictionary(x => x.Id);
        var childCounts = allCategories.Where(x => x.ParentId.HasValue).GroupBy(x => x.ParentId!.Value)
            .ToDictionary(x => x.Key, x => x.Count());
        var productRows = await (from mapping in mappings where pageIds.Contains(mapping.CategoryId) select mapping.CategoryId)
            .ToListAsync(cancellationToken);
        var publishedProductRows = await (from mapping in mappings
                                          join product in products on mapping.ProductId equals product.Id
                                          where pageIds.Contains(mapping.CategoryId) && product.Status == ProductStatus.Published
                                          select mapping.CategoryId).ToListAsync(cancellationToken);
        var productCounts = productRows.GroupBy(x => x).ToDictionary(x => x.Key, x => x.Count());
        var publishedCounts = publishedProductRows.GroupBy(x => x).ToDictionary(x => x.Key, x => x.Count());
        var items = page.Select(category => CatalogCategoryManagementMapper.Map(
            category,
            category.ParentId.HasValue && parents.TryGetValue(category.ParentId.Value, out var parent)
                ? new CatalogCategoryParentDto(parent.Id, parent.Name, parent.Slug)
                : null,
            childCounts.GetValueOrDefault(category.Id),
            productCounts.GetValueOrDefault(category.Id),
            publishedCounts.GetValueOrDefault(category.Id))).ToList();
        return TResult<PaginatedList<CatalogCategoryManagementDto>>.Success(
            PaginatedList<CatalogCategoryManagementDto>.Create(items, total, request.Page, request.PageSize));
    }
}
