using System.Text.Json;
using Ecom.Application.Common.Interfaces;
using Ecom.Application.Features.Demo.QrLogin;
using Microsoft.Extensions.Caching.Distributed;

namespace Ecom.Infrastructure.Caching;

public sealed class DemoQrLoginStore(
    IDistributedCache cache,
    IDistributedLockService distributedLock,
    ILogger<DemoQrLoginStore> logger) : IDemoQrLoginStore
{
    private const string CachePrefix = "demo:qr-login:";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public Task CreateAsync(DemoQrLoginAttempt attempt, CancellationToken cancellationToken = default) =>
        WriteAsync(attempt, cancellationToken);

    public async Task<DemoQrLoginAttempt?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var json = await cache.GetStringAsync(CacheKey(id), cancellationToken);
            return string.IsNullOrWhiteSpace(json)
                ? null
                : JsonSerializer.Deserialize<DemoQrLoginAttempt>(json, JsonOptions);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Demo QR cache read failed.");
            throw new DemoQrLoginStoreUnavailableException("Demo QR cache is unavailable.", exception);
        }
    }

    public async Task<DemoQrLoginTransitionResult> TryTransitionAsync(
        Guid id,
        DemoQrLoginStatus targetStatus,
        Guid userId,
        DateTime now,
        CancellationToken cancellationToken = default)
    {
        await using var handle = await distributedLock.TryAcquireAsync(
            $"{CachePrefix}lock:{id:N}", TimeSpan.FromSeconds(1), cancellationToken);
        if (handle is null)
            return DemoQrLoginTransitionResult.Busy;

        var attempt = await GetAsync(id, cancellationToken);
        if (attempt is null || attempt.ExpiresAt <= now)
        {
            await cache.RemoveAsync(CacheKey(id), cancellationToken);
            return DemoQrLoginTransitionResult.MissingOrExpired;
        }

        if (attempt.Status != DemoQrLoginStatus.Pending)
            return DemoQrLoginTransitionResult.AlreadyCompleted;

        var updated = attempt with
        {
            Status = targetStatus,
            ApprovedUserId = targetStatus == DemoQrLoginStatus.Approved ? userId : null,
            ApprovedAt = now
        };
        await WriteAsync(updated, cancellationToken);
        return DemoQrLoginTransitionResult.Updated;
    }

    private async Task WriteAsync(DemoQrLoginAttempt attempt, CancellationToken cancellationToken)
    {
        var ttl = attempt.ExpiresAt - DateTime.UtcNow;
        if (ttl <= TimeSpan.Zero)
            return;

        try
        {
            var json = JsonSerializer.Serialize(attempt, JsonOptions);
            await cache.SetStringAsync(CacheKey(attempt.Id), json,
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl }, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Demo QR cache write failed.");
            throw new DemoQrLoginStoreUnavailableException("Demo QR cache is unavailable.", exception);
        }
    }

    private static string CacheKey(Guid id) => $"{CachePrefix}{id:N}";
}
