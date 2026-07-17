namespace Ecom.Domain.Entities;
public class TraceEvent : BaseEntity
{
    public Guid TraceLotId { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public DateTime OccurredAt { get; private set; }
    public string? LocationText { get; private set; }
    public string? Description { get; private set; }
    public Guid? VerifiedByUserId { get; private set; }

    private TraceEvent()
    {
    }
}