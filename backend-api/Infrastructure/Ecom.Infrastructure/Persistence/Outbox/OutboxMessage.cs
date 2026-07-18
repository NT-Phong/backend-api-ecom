namespace Ecom.Infrastructure.Persistence.Outbox;

public sealed class OutboxMessage
{
    public Guid Id { get; private set; }
    public DateTimeOffset OccurredOn { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public DateTimeOffset? ProcessedAt { get; private set; }
    public int RetryCount { get; private set; }
    public DateTimeOffset? NextAttemptAt { get; private set; }
    public string? LastError { get; private set; }

    public static OutboxMessage Create(Guid id, DateTimeOffset occurredOn, string eventType, string payload) =>
        new() { Id = id, OccurredOn = occurredOn, EventType = eventType, Payload = payload };

    private OutboxMessage() { }
}
