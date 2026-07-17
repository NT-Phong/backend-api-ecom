using System.Collections.Concurrent;
using Ecom.Application.Common.Interfaces;

namespace Ecom.Infrastructure.Locking;

/// <summary>
/// Temporary lock implementation backed by in-memory semaphores.
/// NOTE: This is process-local and NOT safe for multi-instance deployments.
/// Replace with Redis/Postgres advisory lock implementation for production scale-out.
/// </summary>
public sealed class InMemoryDistributedLockService : IDistributedLockService
{
    private sealed class LockEntry
    {
        public SemaphoreSlim Semaphore { get; } = new(1, 1);
        public int RefCount;
    }

    private sealed class LockHandle(string key, LockEntry entry) : IAsyncDisposable
    {
        private int _disposed;

        public ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 1)
                return ValueTask.CompletedTask;

            entry.Semaphore.Release();

            if (Interlocked.Decrement(ref entry.RefCount) == 0)
            {
                if (Locks.TryRemove(new KeyValuePair<string, LockEntry>(key, entry)))
                {
                    entry.Semaphore.Dispose();
                }
            }

            return ValueTask.CompletedTask;
        }
    }

    private static readonly ConcurrentDictionary<string, LockEntry> Locks = new();

    public async Task<IAsyncDisposable?> TryAcquireAsync(
        string key,
        TimeSpan waitTimeout,
        CancellationToken cancellationToken = default)
    {
        while (true)
        {
            var entry = Locks.GetOrAdd(key, _ => new LockEntry());
            Interlocked.Increment(ref entry.RefCount);

            try
            {
                var acquired = await entry.Semaphore.WaitAsync(waitTimeout, cancellationToken);
                if (!acquired)
                {
                    if (Interlocked.Decrement(ref entry.RefCount) == 0)
                    {
                        Locks.TryRemove(new KeyValuePair<string, LockEntry>(key, entry));
                    }

                    return null;
                }

                return new LockHandle(key, entry);
            }
            catch (ObjectDisposedException)
            {
                if (Interlocked.Decrement(ref entry.RefCount) == 0)
                {
                    Locks.TryRemove(new KeyValuePair<string, LockEntry>(key, entry));
                }
            }
        }
    }
}

