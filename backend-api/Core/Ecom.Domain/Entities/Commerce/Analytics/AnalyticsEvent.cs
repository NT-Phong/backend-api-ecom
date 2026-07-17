namespace Ecom.Domain.Entities;
public class AnalyticsEvent : BaseEntity
{
    public Guid? VisitorSessionId { get; private set; }
    public Guid? ProductId { get; private set; }
    public Guid? CampaignId { get; private set; }
    public AnalyticsEventType EventType { get; private set; }
    public string? Path { get; private set; }
    public string? SearchTerm { get; private set; }
    public DateTime OccurredAt { get; private set; }

    private AnalyticsEvent()
    {
    }
}