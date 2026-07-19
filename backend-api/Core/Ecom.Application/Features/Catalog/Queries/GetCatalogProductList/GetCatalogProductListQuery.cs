using Ecom.Application.Features.Catalog.Common;
using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Catalog.Queries.GetCatalogProductList;

public sealed class GetCatalogProductListQuery : IRequest<TResult<PaginatedList<CatalogProductListItemDto>>>
{
    public string? Q { get; init; }
    public ProductStatus? Status { get; init; }
    public Guid? ProducerId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public int Skip() => (Page - 1) * PageSize;
}

public sealed class GetCatalogProductListQueryValidator : AbstractValidator<GetCatalogProductListQuery>
{
    public GetCatalogProductListQueryValidator()
    {
        RuleFor(x => x.Q).MaximumLength(300);
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
    }
}

public sealed class GetCatalogProductListQueryHandler(IUnitOfWork unitOfWork, ICatalogProductAccessService access)
    : IRequestHandler<GetCatalogProductListQuery, TResult<PaginatedList<CatalogProductListItemDto>>>
{
    public async Task<TResult<PaginatedList<CatalogProductListItemDto>>> Handle(GetCatalogProductListQuery request,
        CancellationToken cancellationToken)
    {
        var authorization = access.Ensure(Permissions.CatalogProducts.Read);
        if (!authorization.IsSuccess)
            return CatalogCommandSupport.Failure<PaginatedList<CatalogProductListItemDto>>(authorization);

        var query = unitOfWork.Repository<Product>().QueryNoTracking();
        if (!string.IsNullOrWhiteSpace(request.Q))
        {
            var search = request.Q.Trim().ToLower();
            query = query.Where(x => x.Name.ToLower().Contains(search) || x.Slug.ToLower().Contains(search));
        }
        if (request.Status.HasValue) query = query.Where(x => x.Status == request.Status.Value);
        if (request.ProducerId.HasValue) query = query.Where(x => x.ProducerId == request.ProducerId.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        if (totalCount == 0)
            return TResult<PaginatedList<CatalogProductListItemDto>>.Success(
                PaginatedList<CatalogProductListItemDto>.Create([], 0, request.Page, request.PageSize));

        var page = await query.OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt).ThenBy(x => x.Id)
            .Skip(request.Skip()).Take(request.PageSize)
            .Select(x => new ProductRow(x.Id, x.ProducerId, x.Name, x.Slug, x.Status, x.CreatedAt, x.UpdatedAt))
            .ToListAsync(cancellationToken);

        var productIds = page.Select(x => x.Id).ToArray();
        var primaryCategories = await (
            from mapping in unitOfWork.Repository<ProductCategory>().QueryNoTracking()
            join category in unitOfWork.Repository<Category>().QueryNoTracking() on mapping.CategoryId equals category.Id
            where productIds.Contains(mapping.ProductId) && mapping.IsPrimary
            select new { mapping.ProductId, Category = new CategorySummaryDto(category.Id, category.Name, category.Slug, true, category.DisplayOrder) })
            .ToDictionaryAsync(x => x.ProductId, x => x.Category, cancellationToken);

        var items = page.Select(x => new CatalogProductListItemDto(x.Id, x.ProducerId, x.Name, x.Slug,
            x.Status, x.CreatedAt, x.UpdatedAt, primaryCategories.GetValueOrDefault(x.Id))).ToList();
        return TResult<PaginatedList<CatalogProductListItemDto>>.Success(
            PaginatedList<CatalogProductListItemDto>.Create(items, totalCount, request.Page, request.PageSize));
    }

    private sealed record ProductRow(Guid Id, Guid ProducerId, string Name, string Slug, ProductStatus Status,
        DateTime CreatedAt, DateTime? UpdatedAt);
}
