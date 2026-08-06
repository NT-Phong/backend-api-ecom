using Ecom.Application.Features.Catalog.Common;
using Ecom.Application.Features.Catalog.Queries.GetProductList;

namespace Ecom.Application.Common.Interfaces;

/// <summary>
/// PostgreSQL-backed read model for anonymous storefront product discovery.
/// It deliberately exposes only published catalog facts, never management data.
/// </summary>
public interface IPublicCatalogReadStore
{
    Task<PaginatedList<ProductListItemDto>> GetProductListAsync(
        GetProductListQuery query,
        CancellationToken cancellationToken = default);
}
