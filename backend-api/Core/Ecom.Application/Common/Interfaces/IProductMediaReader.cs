using Ecom.Application.Common.Models;

namespace Ecom.Application.Common.Interfaces;

public interface IProductMediaReader
{
    Task<IReadOnlyList<ProductMediaDto>> GetPublicMediaAsync(Guid productId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, ProductMediaDto>> GetPrimaryPublicMediaAsync(
        IReadOnlyCollection<Guid> productIds, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, ProductMediaDto>> GetPrimaryCleanPublicMediaAsync(
        IReadOnlyCollection<Guid> productIds, CancellationToken cancellationToken = default);
}
