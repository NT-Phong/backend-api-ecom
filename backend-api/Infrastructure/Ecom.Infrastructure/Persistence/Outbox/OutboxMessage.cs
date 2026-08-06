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
    public Guid? LeaseToken { get; private set; }
    public DateTimeOffset? LeaseExpiresAt { get; private set; }
    public DateTimeOffset? DeadLetteredAt { get; private set; }

    public static OutboxMessage Create(Guid id, DateTimeOffset occurredOn, string eventType, string payload) =>
        new() { Id = id, OccurredOn = occurredOn, EventType = eventType, Payload = payload };

    public void Claim(Guid leaseToken, DateTimeOffset leaseExpiresAt)
    {
        LeaseToken = leaseToken;
        LeaseExpiresAt = leaseExpiresAt;
    }

    public void MarkProcessed(Guid leaseToken, DateTimeOffset processedAt)
    {
        EnsureLease(leaseToken);
        ProcessedAt = processedAt;
        LeaseToken = null;
        LeaseExpiresAt = null;
        LastError = null;
    }

    public void ScheduleRetry(Guid leaseToken, DateTimeOffset nextAttemptAt, string error, int maxRetries,
        DateTimeOffset occurredAt)
    {
        EnsureLease(leaseToken);
        RetryCount++;
        LeaseToken = null;
        LeaseExpiresAt = null;
        LastError = error.Length <= 2000 ? error : error[..2000];
        if (RetryCount >= maxRetries)
        {
            DeadLetteredAt = occurredAt;
            NextAttemptAt = null;
            return;
        }

        NextAttemptAt = nextAttemptAt;
    }

    private void EnsureLease(Guid leaseToken)
    {
        if (leaseToken == Guid.Empty || LeaseToken != leaseToken)
            throw new InvalidOperationException("Outbox message lease ownership was lost.");
    }

    private OutboxMessage() { }
}
