using Ecom.Application.Common.Interfaces;
using StackExchange.Redis;

namespace Ecom.Infrastructure.Locking;

/// <summary>
/// Distributed lock implementation backed by Redis using LockTake/LockRelease.
/// Safe for multi-instance deployments.
/// </summary>
public sealed class RedisDistributedLockService : IDistributedLockService
{
    private readonly IConnectionMultiplexer _redis;

    public RedisDistributedLockService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task<IAsyncDisposable?> TryAcquireAsync(
        string key,
        TimeSpan waitTimeout,
        CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var lockKey = $"lock:{key}";
        var lockValue = Guid.NewGuid().ToString("N");

        var deadline = DateTime.UtcNow.Add(waitTimeout);
        var lockExpiry = TimeSpan.FromSeconds(30); // Cố định TTL 30s thay vì phụ thuộc vào delay

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var acquired = await db.LockTakeAsync(lockKey, lockValue, lockExpiry);
            if (acquired)
            {
                return new RedisLockHandle(db, lockKey, lockValue);
            }

            var remaining = deadline - DateTime.UtcNow;
            if (remaining <= TimeSpan.Zero) break;

            var delay = remaining < TimeSpan.FromMilliseconds(50) ? remaining : TimeSpan.FromMilliseconds(50);
            await Task.Delay(delay, cancellationToken);
        }

        return null;
    }

    // Inner handle: releases the Redis lock on dispose
    private sealed class RedisLockHandle : IAsyncDisposable
    {
        private readonly StackExchange.Redis.IDatabase _db;
        private readonly string _key;
        private readonly string _value;
        private int _disposed;

        public RedisLockHandle(StackExchange.Redis.IDatabase db, string key, string value)
        {
            _db = db;
            _key = key;
            _value = value;
        }

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 1)
                return;

            await _db.LockReleaseAsync(_key, _value);
        }
    }
}
