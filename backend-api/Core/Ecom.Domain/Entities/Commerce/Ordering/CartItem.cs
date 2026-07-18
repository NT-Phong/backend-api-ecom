namespace Ecom.Domain.Entities;
public class CartItem : BaseEntity
{
    public Guid CartId { get; private set; }
    public Guid ProductVariantId { get; private set; }
    public int Quantity { get; private set; }

    internal static CartItem Create(Guid cartId, Guid productVariantId, int quantity)
    {
        if (cartId == Guid.Empty || productVariantId == Guid.Empty)
            throw new CommerceDomainException("CART_ITEM_REFERENCE_REQUIRED", "Cart and variant are required.");
        EnsureQuantity(quantity);
        return new CartItem { CartId = cartId, ProductVariantId = productVariantId, Quantity = quantity };
    }

    internal void Increase(int quantity)
    {
        EnsureQuantity(quantity);
        checked { Quantity += quantity; }
    }

    internal void ChangeQuantity(int quantity)
    {
        EnsureQuantity(quantity);
        Quantity = quantity;
    }

    private static void EnsureQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new CommerceDomainException("CART_QUANTITY_INVALID", "Cart quantity must be greater than zero.");
    }

    private CartItem()
    {
    }
}
