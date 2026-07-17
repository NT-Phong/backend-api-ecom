namespace Ecom.Domain.Entities;
public class ProductQuestion : BaseEntity
{
    public Guid ProductId { get; private set; }
    public Guid? UserId { get; private set; }
    public string? GuestName { get; private set; }
    public string? GuestEmail { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public QuestionStatus Status { get; private set; }
    public DateTime AskedAt { get; private set; }

    private ProductQuestion()
    {
    }
}