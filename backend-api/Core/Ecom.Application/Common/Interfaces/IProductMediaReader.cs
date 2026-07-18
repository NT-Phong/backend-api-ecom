using Ecom.Application.Common.Models;

namespace Ecom.Application.Common.Interfaces;

public interface IProductMediaReader
{
    Task<IReadOnlyList<ProductMediaDto>> GetPublicMediaAsync(Guid productId,
        CancellationToken cancellationToken = default);
}
