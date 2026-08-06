using Ecom.Infrastructure.Persistence.Database;
using Ecom.Infrastructure.Persistence.Outbox;

namespace Ecom.Infrastructure.Services;

public sealed class OutboxProcessor(ApplicationDbContext db, OutboxMessageDispatcher dispatcher,
    IOptions<OutboxProcessorOptions> options, ILogger<OutboxProcessor> logger)
{
    public async Task<int> ProcessBatchAsync(CancellationToken cancellationToken)
    {
        var settings = options.Value;
        var leaseToken = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var messages = await ClaimAsync(leaseToken, now, settings, cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                await dispatcher.DispatchAsync(message, cancellationToken);
                message.MarkProcessed(leaseToken, DateTimeOffset.UtcNow);
                await db.SaveChangesAsync(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                var occurredAt = DateTimeOffset.UtcNow;
                var retryDelay = TimeSpan.FromSeconds(Math.Min(3600, Math.Pow(2, Math.Min(message.RetryCount, 10))));
                message.ScheduleRetry(leaseToken, occurredAt.Add(retryDelay), exception.GetType().Name,
                    settings.ValidMaxRetries, occurredAt);
                await db.SaveChangesAsync(cancellationToken);
                logger.LogWarning("Outbox message {OutboxMessageId} dispatch failed; retry count is {RetryCount}.",
                    message.Id, message.RetryCount);
            }
        }

        return messages.Count;
    }

    private async Task<List<OutboxMessage>> ClaimAsync(Guid leaseToken, DateTimeOffset now,
        OutboxProcessorOptions settings, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var messages = await db.OutboxMessages.FromSqlInterpolated($"""
            SELECT * FROM "Tbl_OutboxMessage"
            WHERE "ProcessedAt" IS NULL
              AND "DeadLetteredAt" IS NULL
              AND ("NextAttemptAt" IS NULL OR "NextAttemptAt" <= {now})
              AND ("LeaseExpiresAt" IS NULL OR "LeaseExpiresAt" <= {now})
            ORDER BY "OccurredOn", "Id"
            FOR UPDATE SKIP LOCKED
            LIMIT {settings.ValidBatchSize}
            """).ToListAsync(cancellationToken);
        foreach (var message in messages)
            message.Claim(leaseToken, now.Add(settings.LeaseDuration));
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return messages;
    }
}
