namespace Ecom.Domain.Entities;
public class Notification : BaseEntity
{
    public string NotificationType { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public string? Data { get; private set; }
    public bool CreatedBySystem { get; private set; }

    public static Notification CreateSystem(string type, string title, string body, string? data = null)
    {
        if (string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(body))
            throw new CommerceDomainException("NOTIFICATION_DETAILS_REQUIRED", "Notification type, title, and body are required.");
        return new Notification
        {
            NotificationType = type.Trim(),
            Title = title.Trim(),
            Body = body.Trim(),
            Data = string.IsNullOrWhiteSpace(data) ? null : data,
            CreatedBySystem = true
        };
    }

    private Notification()
    {
    }
}
