namespace Ecom.Application.Features.Catalog.Products.Services;

public interface ICatalogAuditWriter
{
    Task WriteAsync(string action, Guid productId, object? before, object? after,
        CancellationToken cancellationToken = default);
}
