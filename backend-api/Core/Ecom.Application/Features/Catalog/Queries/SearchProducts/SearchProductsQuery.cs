using Ecom.Application.Features.Catalog.Common;

namespace Ecom.Application.Features.Catalog.Queries.SearchProducts;

public sealed record SearchProductsQuery : IRequest<TResult<ProductSearchResponseDto>>
{
    public string Q { get; init; } = string.Empty;
    public string? CategorySlug { get; init; }
    public Guid? ProducerId { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public CatalogAvailabilityStatus? Availability { get; init; }
    public string Sort { get; init; } = ProductSearchSort.Relevance;
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public int Skip() => (Page - 1) * PageSize;
}

public sealed class SearchProductsQueryValidator : AbstractValidator<SearchProductsQuery>
{
    public SearchProductsQueryValidator()
    {
        RuleFor(x => x.Q).NotEmpty().Length(2, 100);
        RuleFor(x => x.CategorySlug).MaximumLength(350);
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
        RuleFor(x => x.MinPrice).GreaterThanOrEqualTo(0).When(x => x.MinPrice.HasValue);
        RuleFor(x => x.MaxPrice).GreaterThanOrEqualTo(0).When(x => x.MaxPrice.HasValue);
        RuleFor(x => x).Must(x => !x.MinPrice.HasValue || !x.MaxPrice.HasValue || x.MinPrice <= x.MaxPrice)
            .WithMessage("Minimum price cannot exceed maximum price.");
        RuleFor(x => x.Sort).Must(ProductSearchSort.IsSupported).WithMessage("Sort is not supported.");
        RuleFor(x => x).Must(x => x.Sort != ProductSearchSort.Relevance || !string.IsNullOrWhiteSpace(x.Q))
            .WithMessage("Relevance sort requires a search term.");
    }
}

public sealed class SearchProductsQueryHandler(IPublicCatalogSearchStore searchStore)
    : IRequestHandler<SearchProductsQuery, TResult<ProductSearchResponseDto>>
{
    public async Task<TResult<ProductSearchResponseDto>> Handle(SearchProductsQuery request, CancellationToken cancellationToken) =>
        TResult<ProductSearchResponseDto>.Success(await searchStore.SearchAsync(request, cancellationToken));
}
