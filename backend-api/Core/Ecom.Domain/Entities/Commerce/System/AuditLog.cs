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

    public static AuditLog Create(Guid? actorUserId, string action, string entityName, Guid entityId,
        string? beforeData, string? afterData, DateTime occurredAt, Guid? correlationId = null,
        string? ipAddress = null)
    {
        if (string.IsNullOrWhiteSpace(action))
            throw new CommerceDomainException("AUDIT_ACTION_REQUIRED", "An audit action is required.");
        if (string.IsNullOrWhiteSpace(entityName))
            throw new CommerceDomainException("AUDIT_ENTITY_REQUIRED", "An audit entity name is required.");
        if (entityId == Guid.Empty)
            throw new CommerceDomainException("AUDIT_ENTITY_ID_REQUIRED", "An audit entity ID is required.");

        return new AuditLog
        {
            ActorUserId = actorUserId,
            Action = action.Trim(),
            EntityName = entityName.Trim(),
            EntityId = entityId,
            BeforeData = beforeData,
            AfterData = afterData,
            OccurredAt = occurredAt,
            CorrelationId = correlationId,
            IpAddress = ipAddress
        };
    }
}
