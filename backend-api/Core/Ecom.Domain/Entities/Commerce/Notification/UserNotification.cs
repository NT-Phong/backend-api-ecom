namespace Ecom.Domain.Entities;
public class UserNotification : BaseEntity
{
    public Guid NotificationId { get; private set; }
    public Guid UserId { get; private set; }
    public NotificationDeliveryStatus DeliveryStatus { get; private set; }
    public DateTime? ReadAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }

    public static UserNotification Create(Guid notificationId, Guid userId)
    {
        if (notificationId == Guid.Empty || userId == Guid.Empty)
            throw new CommerceDomainException("USER_NOTIFICATION_REFERENCE_REQUIRED", "Notification and user are required.");
        return new UserNotification
        {
            NotificationId = notificationId,
            UserId = userId,
            DeliveryStatus = NotificationDeliveryStatus.Pending
        };
    }

    public void MarkDelivered(DateTime deliveredAt)
    {
        if (DeliveryStatus != NotificationDeliveryStatus.Pending || deliveredAt == default)
            throw new CommerceDomainException("NOTIFICATION_DELIVERY_TRANSITION_INVALID", "Only pending notifications can be delivered.");
        DeliveryStatus = NotificationDeliveryStatus.Delivered;
        DeliveredAt = deliveredAt;
    }

    public void MarkFailed()
    {
        if (DeliveryStatus != NotificationDeliveryStatus.Pending)
            throw new CommerceDomainException("NOTIFICATION_DELIVERY_TRANSITION_INVALID", "Only pending notifications can fail.");
        DeliveryStatus = NotificationDeliveryStatus.Failed;
    }

    public void MarkRead(DateTime readAt)
    {
        if (DeliveryStatus is not (NotificationDeliveryStatus.Delivered or NotificationDeliveryStatus.Read) || readAt == default)
            throw new CommerceDomainException("NOTIFICATION_READ_TRANSITION_INVALID", "Only delivered notifications can be read.");
        DeliveryStatus = NotificationDeliveryStatus.Read;
        ReadAt ??= readAt;
    }

    private UserNotification()
    {
    }
}
