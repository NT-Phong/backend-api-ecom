using Ecom.Application.Features.Catalog.Common;

namespace Ecom.Application.Features.Catalog.Queries.SearchProducts;

public sealed record ProductSearchSuggestionsQuery : IRequest<TResult<IReadOnlyList<ProductSearchSuggestionDto>>>
{
    public string Q { get; init; } = string.Empty;
}

public sealed class ProductSearchSuggestionsQueryValidator : AbstractValidator<ProductSearchSuggestionsQuery>
{
    public ProductSearchSuggestionsQueryValidator() => RuleFor(x => x.Q).NotEmpty().Length(2, 100);
}

public sealed class ProductSearchSuggestionsQueryHandler(IPublicCatalogSearchStore searchStore)
    : IRequestHandler<ProductSearchSuggestionsQuery, TResult<IReadOnlyList<ProductSearchSuggestionDto>>>
{
    public async Task<TResult<IReadOnlyList<ProductSearchSuggestionDto>>> Handle(ProductSearchSuggestionsQuery request,
        CancellationToken cancellationToken) => TResult<IReadOnlyList<ProductSearchSuggestionDto>>.Success(
        await searchStore.SuggestAsync(request, cancellationToken));
}
