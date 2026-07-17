namespace Ecom.Domain.Entities;
public class Cart : BaseEntity
{
    public Guid? UserId { get; private set; }
    public string? GuestTokenHash { get; private set; }
    public CartStatus Status { get; private set; }
    public string CurrencyCode { get; private set; } = "VND";
    public DateTime? ExpiresAt { get; private set; }

    private Cart()
    {
    }
}