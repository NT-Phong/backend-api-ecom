namespace Ecom.Domain.Entities;
public class OrderStatusHistory : BaseEntity
{
    public Guid OrderId { get; private set; }
    public OrderStatus? FromStatus { get; private set; }
    public OrderStatus ToStatus { get; private set; }
    public string? Reason { get; private set; }
    public Guid? ChangedByUserId { get; private set; }
    public DateTime ChangedAt { get; private set; }

    internal static OrderStatusHistory Create(Guid orderId, OrderStatus? from, OrderStatus to, string? reason, Guid? actorId, DateTime changedAt) =>
        new()
        {
            OrderId = orderId,
            FromStatus = from,
            ToStatus = to,
            Reason = reason?.Trim(),
            ChangedByUserId = actorId,
            ChangedAt = changedAt
        };

    private OrderStatusHistory()
    {
    }
}
