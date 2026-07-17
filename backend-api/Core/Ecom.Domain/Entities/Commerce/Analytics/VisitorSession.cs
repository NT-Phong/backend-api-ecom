namespace Ecom.Domain.Entities;
public class VisitorSession : BaseEntity
{
    public Guid? UserId { get; private set; }
    public string SessionHash { get; private set; } = string.Empty;
    public string? Source { get; private set; }
    public string? Medium { get; private set; }
    public string? Campaign { get; private set; }
    public ConsentStatus ConsentStatus { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? EndedAt { get; private set; }

    private VisitorSession()
    {
    }
}