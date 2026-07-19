using Ecom.Application.Features.Catalog.Common;

namespace Ecom.Application.Features.Catalog.Queries.GetProductList;

public sealed record GetProductListQuery : IRequest<TResult<PaginatedList<ProductListItemDto>>>
{
    public string? Q { get; init; }
    public string? CategorySlug { get; init; }
    public Guid? ProducerId { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public string Sort { get; init; } = ProductSort.Newest;
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public int Skip() => (Page - 1) * PageSize;
}

public sealed class GetProductListQueryValidator : AbstractValidator<GetProductListQuery>
{
    public GetProductListQueryValidator()
    {
        RuleFor(x => x.Q).MaximumLength(200);
        RuleFor(x => x.CategorySlug).MaximumLength(250);
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
        RuleFor(x => x.MinPrice).GreaterThanOrEqualTo(0).When(x => x.MinPrice.HasValue);
        RuleFor(x => x.MaxPrice).GreaterThanOrEqualTo(0).When(x => x.MaxPrice.HasValue);
        RuleFor(x => x).Must(x => !x.MinPrice.HasValue || !x.MaxPrice.HasValue || x.MinPrice <= x.MaxPrice)
            .WithMessage("Minimum price cannot exceed maximum price.");
        RuleFor(x => x.Sort).Must(ProductSort.IsSupported).WithMessage("Sort is not supported.");
    }
}
