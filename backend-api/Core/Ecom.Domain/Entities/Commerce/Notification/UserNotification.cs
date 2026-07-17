namespace Ecom.Domain.Entities;
public class UserNotification : BaseEntity
{
    public Guid NotificationId { get; private set; }
    public Guid UserId { get; private set; }
    public NotificationDeliveryStatus DeliveryStatus { get; private set; }
    public DateTime? ReadAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }

    private UserNotification()
    {
    }
}