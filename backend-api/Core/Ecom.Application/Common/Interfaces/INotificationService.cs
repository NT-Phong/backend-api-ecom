namespace Ecom.Application.Common.Interfaces;

/// <summary>Application boundary for persisted or push notifications. Delivery is implemented only when a feature requires it.</summary>
public interface INotificationService
{
    Task NotifyAsync(Guid? recipientId, string title, string message, string type = "info", string? targetUrl = null, CancellationToken cancellationToken = default, string category = "notification");
    Task NotifyBulkAsync(IReadOnlyList<Guid> recipientIds, string title, string message, string type = "info", string? targetUrl = null, CancellationToken cancellationToken = default, string category = "notification");
}
