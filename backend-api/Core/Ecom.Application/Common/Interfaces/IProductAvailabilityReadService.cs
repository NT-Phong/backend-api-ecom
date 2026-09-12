using Ecom.Application.Features.Catalog.Common;

namespace Ecom.Application.Common.Interfaces;

public interface IProductAvailabilityReadService
{
    Task<IReadOnlyDictionary<Guid, CatalogAvailabilityStatus>> ResolveForVariantsAsync(
        IReadOnlyCollection<Guid> variantIds, DateTime asOfUtc, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, CatalogAvailabilityStatus>> ResolveForProductsAsync(
        IReadOnlyCollection<Guid> productIds, DateTime asOfUtc, CancellationToken cancellationToken = default);

    CatalogAvailabilityStatus SummarizeProduct(IEnumerable<CatalogAvailabilityStatus> variantAvailability);
}
