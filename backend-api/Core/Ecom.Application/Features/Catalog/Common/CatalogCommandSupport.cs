using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Catalog.Common;

internal static class CatalogCommandSupport
{
    internal static TResult<T> Failure<T>(TResult result) => TResult<T>.Failure(result.Error!, result.ErrorCode);

    internal static TResult? EnsureVersion(Product product, Guid concurrencyStamp) =>
        concurrencyStamp == Guid.Empty || product.ConcurrencyStamp != concurrencyStamp
            ? TResult.Failure(MessageKey.DataHasBeenChanged, ErrorCodes.ALREADY_EXISTS)
            : null;

    internal static ProductManagementResult RenewVersion(Product product) =>
        new(product.Id, product.Slug, product.Status, product.RenewConcurrencyStamp());
}
