using StackExchange.Redis;
using Ecom.Application.Common.Configuration;
using Microsoft.Extensions.Options;

namespace Ecom.Infrastructure.Security;

public sealed record AuthRateLimitCounter(long Count, TimeSpan TimeToLive);

public interface IAuthRateLimitCounterStore
{
    Task<AuthRateLimitCounter?> IncrementAsync(
        string key,
        TimeSpan window,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Redis-backed atomic fixed-window counter. The Lua script is evaluated by Redis as one operation,
/// so all API instances share the same count and expiry.
/// </summary>
public sealed class RedisAuthRateLimitCounterStore(
    IConnectionMultiplexer redis,
    IOptions<AuthRateLimitOptions> options,
    ILogger<RedisAuthRateLimitCounterStore> logger) : IAuthRateLimitCounterStore
{
    private const string FixedWindowScript = """
        local current = redis.call('INCR', KEYS[1])
        if current == 1 then
            redis.call('PEXPIRE', KEYS[1], ARGV[1])
        end
        local ttl = redis.call('PTTL', KEYS[1])
        return { current, ttl }
        """;

    public async Task<AuthRateLimitCounter?> IncrementAsync(
        string key,
        TimeSpan window,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var operation = redis.GetDatabase().ScriptEvaluateAsync(
                FixedWindowScript,
                [(RedisKey)key],
                [(RedisValue)Math.Max(1L, (long)window.TotalMilliseconds)]);
            var timeout = TimeSpan.FromMilliseconds(Math.Clamp(options.Value.RedisOperationTimeoutMilliseconds, 50, 5000));
            var result = (RedisResult[]?)await operation.WaitAsync(timeout, cancellationToken);

            if (result is null || result.Length != 2)
                return null;

            return new AuthRateLimitCounter(
                (long)result[0],
                TimeSpan.FromMilliseconds(Math.Max(1L, (long)result[1])));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (TimeoutException)
        {
            logger.LogWarning("Redis auth rate-limit counter timed out");
            return null;
        }
        catch (Exception)
        {
            logger.LogWarning("Redis auth rate-limit counter unavailable");
            return null;
        }
    }
}
