using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Catalog.Products.Services;

/// <summary>
/// Product-scoped audit writer. Callers pass only safe snapshots; this class never accepts
/// storage keys, private URLs, credentials, or customer data.
/// </summary>
public sealed class CatalogAuditWriter(IUnitOfWork unitOfWork, ICurrentUser currentUser) : ICatalogAuditWriter
{
    public Task WriteAsync(string action, Guid productId, object? before, object? after,
        CancellationToken cancellationToken = default)
    {
        var audit = AuditLog.Create(
            currentUser.IsAuthenticated ? currentUser.UserId : null,
            action,
            "CatalogProduct",
            productId,
            before is null ? null : JsonSerializer.Serialize(before),
            after is null ? null : JsonSerializer.Serialize(after),
            DateTime.UtcNow);
        return unitOfWork.Repository<AuditLog>().InsertAsync(audit, cancellationToken);
    }
}
