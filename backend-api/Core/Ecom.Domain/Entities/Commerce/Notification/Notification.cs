namespace Ecom.Domain.Entities;
public class Notification : BaseEntity
{
    public string NotificationType { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public string? Data { get; private set; }
    public bool CreatedBySystem { get; private set; }

    private Notification()
    {
    }
}