namespace Ecom.Domain.Common;

public abstract class BaseEvent : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; protected set; } = DateTime.UtcNow;
    public Guid EventId { get; protected set; } = Guid.NewGuid();
} 

