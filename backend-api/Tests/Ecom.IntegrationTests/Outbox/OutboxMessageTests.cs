using Ecom.Infrastructure.Persistence.Outbox;

namespace Ecom.IntegrationTests.Outbox;

public sealed class OutboxMessageTests
{
    [Fact]
    public void Failed_delivery_retries_then_dead_letters_without_losing_lease_ownership()
    {
        var message = OutboxMessage.Create(Guid.NewGuid(), DateTimeOffset.UtcNow, "event", "{}");
        var lease = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        message.Claim(lease, now.AddMinutes(1));
        message.ScheduleRetry(lease, now.AddSeconds(5), "TransientFailure", maxRetries: 2, now);

        Assert.Equal(1, message.RetryCount);
        Assert.Null(message.DeadLetteredAt);
        Assert.NotNull(message.NextAttemptAt);

        message.Claim(lease, now.AddMinutes(2));
        message.ScheduleRetry(lease, now.AddSeconds(10), "PermanentFailure", maxRetries: 2, now);

        Assert.Equal(2, message.RetryCount);
        Assert.Equal(now, message.DeadLetteredAt);
        Assert.Null(message.NextAttemptAt);
    }

    [Fact]
    public void Processed_delivery_requires_the_claiming_lease()
    {
        var message = OutboxMessage.Create(Guid.NewGuid(), DateTimeOffset.UtcNow, "event", "{}");
        var lease = Guid.NewGuid();
        message.Claim(lease, DateTimeOffset.UtcNow.AddMinutes(1));

        Assert.Throws<InvalidOperationException>(() => message.MarkProcessed(Guid.NewGuid(), DateTimeOffset.UtcNow));

        message.MarkProcessed(lease, DateTimeOffset.UtcNow);
        Assert.NotNull(message.ProcessedAt);
    }
}
