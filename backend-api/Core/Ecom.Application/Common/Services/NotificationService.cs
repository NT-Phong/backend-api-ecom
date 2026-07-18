using Ecom.Domain.Entities;

namespace Ecom.Application.Common.Services;

/// <summary>Persists inbox notifications. External delivery is intentionally handled by a future outbox processor.</summary>
public sealed class NotificationService(IUnitOfWork unitOfWork) : INotificationService
{
    public Task NotifyAsync(Guid? recipientId, string title, string message, string type = "info",
        string? targetUrl = null, CancellationToken cancellationToken = default, string category = "notification")
    {
        if (!recipientId.HasValue || recipientId == Guid.Empty)
            throw new CommerceDomainException("NOTIFICATION_RECIPIENT_REQUIRED", "A notification recipient is required.");
        return NotifyBulkAsync([recipientId.Value], title, message, type, targetUrl, cancellationToken, category);
    }

    public async Task NotifyBulkAsync(IReadOnlyList<Guid> recipientIds, string title, string message,
        string type = "info", string? targetUrl = null, CancellationToken cancellationToken = default,
        string category = "notification")
    {
        var recipients = recipientIds.Where(x => x != Guid.Empty).Distinct().ToList();
        if (recipients.Count == 0)
            throw new CommerceDomainException("NOTIFICATION_RECIPIENT_REQUIRED", "At least one recipient is required.");

        var data = JsonSerializer.Serialize(new { category, targetUrl });
        var notification = Notification.CreateSystem(type, title, message, data);
        await unitOfWork.Repository<Notification>().InsertAsync(notification, cancellationToken);
        await unitOfWork.Repository<UserNotification>().InsertRangeAsync(
            recipients.Select(userId => UserNotification.Create(notification.Id, userId)), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
