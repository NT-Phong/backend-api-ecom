namespace Ecom.Domain.Entities;
public class OrderStatusHistory : BaseEntity
{
    public Guid OrderId { get; private set; }
    public OrderStatus? FromStatus { get; private set; }
    public OrderStatus ToStatus { get; private set; }
    public string? Reason { get; private set; }
    public Guid? ChangedByUserId { get; private set; }
    public DateTime ChangedAt { get; private set; }

    private OrderStatusHistory()
    {
    }
}