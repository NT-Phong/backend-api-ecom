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
}