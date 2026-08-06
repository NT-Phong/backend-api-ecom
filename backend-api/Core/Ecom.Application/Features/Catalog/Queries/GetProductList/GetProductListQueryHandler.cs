using Ecom.Application.Common.Interfaces;
using Ecom.Application.Features.Catalog.Common;

namespace Ecom.Application.Features.Catalog.Queries.GetProductList;

public sealed class GetProductListQueryHandler(
    IPublicCatalogReadStore publicCatalogReadStore)
    : IRequestHandler<GetProductListQuery, TResult<PaginatedList<ProductListItemDto>>>
{
    public async Task<TResult<PaginatedList<ProductListItemDto>>> Handle(GetProductListQuery request,
        CancellationToken cancellationToken)
    {
        var result = await publicCatalogReadStore.GetProductListAsync(request, cancellationToken);
        return TResult<PaginatedList<ProductListItemDto>>.Success(result);
    }
}
