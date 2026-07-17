namespace Ecom.Domain.Common;

public interface IDomainEvent : INotification
{
    DateTimeOffset OccurredOn { get; }
    Guid EventId { get; }
}

