namespace Ecom.Domain.Entities;
public class AuditLog : BaseEntity
{
    public Guid? ActorUserId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string EntityName { get; private set; } = string.Empty;
    public Guid? EntityId { get; private set; }
    public Guid? CorrelationId { get; private set; }
    public string? BeforeData { get; private set; }
    public string? AfterData { get; private set; }
    public DateTime OccurredAt { get; private set; }
    public string? IpAddress { get; private set; }

    private AuditLog()
    {
    }
}