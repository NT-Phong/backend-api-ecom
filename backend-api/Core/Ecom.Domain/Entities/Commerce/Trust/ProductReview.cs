namespace Ecom.Domain.Entities;
public class ProductReview : BaseEntity
{
    public Guid ProductId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid? OrderItemId { get; private set; }
    public int Rating { get; private set; }
    public string? Title { get; private set; }
    public string? Content { get; private set; }
    public ReviewModerationStatus ModerationStatus { get; private set; }
    public DateTime ReviewedAt { get; private set; }

    private ProductReview()
    {
    }
}