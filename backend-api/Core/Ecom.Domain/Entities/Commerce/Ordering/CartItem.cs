namespace Ecom.Domain.Entities;
public class CartItem : BaseEntity
{
    public Guid CartId { get; private set; }
    public Guid ProductVariantId { get; private set; }
    public int Quantity { get; private set; }

    private CartItem()
    {
    }
}