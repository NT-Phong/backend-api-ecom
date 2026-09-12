using Ecom.Application.Features.Catalog.Common;

namespace Ecom.Application.Features.Catalog.Products.Services;

public interface ICatalogReadinessService
{
    Task<CatalogProductReadinessDto?> GetAsync(Guid productId, CancellationToken cancellationToken = default);
}

public sealed record CatalogReadinessCheckDto(string Code, bool Passed);
public sealed record CatalogProductReadinessDto(Guid ProductId, bool CanPublish, bool CanSell,
    IReadOnlyList<CatalogReadinessCheckDto> Checks);
