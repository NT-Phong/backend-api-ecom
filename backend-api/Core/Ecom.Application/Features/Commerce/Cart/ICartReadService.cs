using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Cart;

public interface ICartReadService
{
    Task<CartDto> BuildAsync(Ecom.Domain.Entities.Cart cart, IReadOnlyCollection<CartItem> items,
        CancellationToken cancellationToken);
}
