using Ecom.Application.Features.Catalog.Common;
using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Catalog.Products.Services;

public sealed class CatalogProductMutationService(IUnitOfWork unitOfWork, ICatalogProductAccessService access)
    : ICatalogProductMutationService
{
    public async Task<TResult<Product>> LoadAsync(Guid productId, Guid concurrencyStamp, string permission,
        CancellationToken cancellationToken)
    {
        var authorization = access.Ensure(permission);
        if (!authorization.IsSuccess) return TResult<Product>.Failure(authorization.Error!, authorization.ErrorCode);

        var product = await unitOfWork.Repository<Product>().FindByIdAsync(productId);
        if (product is null) return TResult<Product>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);

        var version = CatalogCommandSupport.EnsureVersion(product, concurrencyStamp);
        return version is null ? TResult<Product>.Success(product) : CatalogCommandSupport.Failure<Product>(version);
    }
}
