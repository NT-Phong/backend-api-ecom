namespace Ecom.Domain.Entities;
public class Wishlist : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = "Default";

    private Wishlist()
    {
    }
}