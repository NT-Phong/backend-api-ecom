using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Catalog.Products.Services;

public interface ICatalogProductMutationService
{
    Task<TResult<Product>> LoadAsync(Guid productId, Guid concurrencyStamp, string permission, CancellationToken cancellationToken);
}
