using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Cart;

public sealed record CartItemDto(Guid Id, Guid ProductVariantId, int Quantity);
public sealed record CartDto(Guid? Id, CartStatus? Status, IReadOnlyList<CartItemDto> Items)
{
    public static CartDto Empty { get; } = new(null, null, []);
}

internal static class CartDtoMapper
{
    internal static CartDto Map(Ecom.Domain.Entities.Cart cart, IEnumerable<CartItem> items) => new(cart.Id, cart.Status,
        items.Where(x => !x.IsDeleted).OrderBy(x => x.CreatedAt).Select(x => new CartItemDto(x.Id, x.ProductVariantId, x.Quantity)).ToList());
}
