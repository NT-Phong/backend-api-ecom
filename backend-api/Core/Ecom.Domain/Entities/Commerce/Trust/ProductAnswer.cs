namespace Ecom.Domain.Entities;
public class ProductAnswer : BaseEntity
{
    public Guid ProductQuestionId { get; private set; }
    public Guid? AnsweredByUserId { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public AnswerStatus Status { get; private set; }
    public DateTime AnsweredAt { get; private set; }

    private ProductAnswer()
    {
    }
}