namespace Ecom.Domain.Events.Commerce;

public sealed class CommerceStateChangedEvent : BaseEvent
{
    public string AggregateType { get; }
    public Guid AggregateId { get; }
    public string? FromState { get; }
    public string ToState { get; }

    public CommerceStateChangedEvent(string aggregateType, Guid aggregateId, string? fromState, string toState)
    {
        AggregateType = aggregateType;
        AggregateId = aggregateId;
        FromState = fromState;
        ToState = toState;
    }
}
