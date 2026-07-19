using System.Collections.Concurrent;

namespace Ecom.Infrastructure.Security;

/// <summary>
/// Process-local counter used only when explicitly selected (Development).
/// It is intentionally not a production fallback because it is not shared across instances.
/// </summary>
public sealed class InMemoryAuthRateLimitCounterStore : IAuthRateLimitCounterStore
{
    private readonly ConcurrentDictionary<string, Entry> _entries = new();

    public Task<AuthRateLimitCounter?> IncrementAsync(
        string key,
        TimeSpan window,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var now = DateTimeOffset.UtcNow;
        var entry = _entries.AddOrUpdate(
            key,
            _ => new Entry(1, now.Add(window)),
            (_, current) => current.ExpiresAt <= now
                ? new Entry(1, now.Add(window))
                : current with { Count = current.Count + 1 });

        return Task.FromResult<AuthRateLimitCounter?>(
            new AuthRateLimitCounter(entry.Count, entry.ExpiresAt - now));
    }

    private sealed record Entry(long Count, DateTimeOffset ExpiresAt);
}
