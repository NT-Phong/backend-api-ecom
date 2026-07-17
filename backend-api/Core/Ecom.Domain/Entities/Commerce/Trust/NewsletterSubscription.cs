namespace Ecom.Domain.Entities;
public class NewsletterSubscription : BaseEntity
{
    public Guid? UserId { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public NewsletterStatus Status { get; private set; }
    public DateTime? ConsentAt { get; private set; }
    public DateTime? UnsubscribedAt { get; private set; }
    public string? Source { get; private set; }

    private NewsletterSubscription()
    {
    }
}