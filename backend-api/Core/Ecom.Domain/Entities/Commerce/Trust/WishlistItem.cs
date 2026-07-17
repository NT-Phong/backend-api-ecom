namespace Ecom.Domain.Entities;
public class WishlistItem : BaseEntity
{
    public Guid WishlistId { get; private set; }
    public Guid ProductId { get; private set; }

    private WishlistItem()
    {
    }
}