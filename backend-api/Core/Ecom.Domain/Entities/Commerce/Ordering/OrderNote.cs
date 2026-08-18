namespace Ecom.Domain.Entities;
public class OrderNote : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Guid? AuthorUserId { get; private set; }
    public OrderNoteType NoteType { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public bool IsVisibleToCustomer { get; private set; }

    private OrderNote()
    {
    }

    public static OrderNote CreateInternal(Guid orderId, Guid authorUserId, string content)
    {
        if (orderId == Guid.Empty || authorUserId == Guid.Empty || string.IsNullOrWhiteSpace(content))
            throw new CommerceDomainException("ORDER_NOTE_REQUIRED", "Order, author, and note content are required.");

        return new OrderNote
        {
            OrderId = orderId,
            AuthorUserId = authorUserId,
            NoteType = OrderNoteType.Internal,
            Content = content.Trim(),
            IsVisibleToCustomer = false
        };
    }
}
