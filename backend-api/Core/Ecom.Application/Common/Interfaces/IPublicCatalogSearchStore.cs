using Ecom.Application.Features.Catalog.Common;
using Ecom.Application.Features.Catalog.Queries.SearchProducts;

namespace Ecom.Application.Common.Interfaces;

public interface IPublicCatalogSearchStore
{
    Task<ProductSearchResponseDto> SearchAsync(SearchProductsQuery query, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductSearchSuggestionDto>> SuggestAsync(ProductSearchSuggestionsQuery query,
        CancellationToken cancellationToken = default);
}
