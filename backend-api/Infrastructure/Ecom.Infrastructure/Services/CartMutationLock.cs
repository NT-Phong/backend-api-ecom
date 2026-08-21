using System.Security.Cryptography;
using System.Text;
using Ecom.Application.Common.Commerce;
using Ecom.Application.Common.Interfaces;
using Ecom.Infrastructure.Persistence.Database;

namespace Ecom.Infrastructure.Services;

/// <summary>
/// Uses PostgreSQL transaction advisory locks to serialize active-cart creation and guest-cart merge.
/// Callers already run inside UnitOfWorkBehavior, so every acquired lock is released on commit or rollback.
/// </summary>
public sealed class CartMutationLock(ApplicationDbContext db) : ICartMutationLock
{
    public Task AcquireAsync(CartPrincipal principal, CancellationToken cancellationToken) =>
        AcquireScopesAsync(CreateScopes(principal.UserId, principal.GuestTokenHash), cancellationToken);

    public Task AcquireForMergeAsync(Guid userId, string guestTokenHash, CancellationToken cancellationToken) =>
        AcquireScopesAsync(CreateScopes(userId, guestTokenHash), cancellationToken);

    private async Task AcquireScopesAsync(IEnumerable<string> scopes, CancellationToken cancellationToken)
    {
        foreach (var key in scopes.Select(ToAdvisoryLockKey).Distinct().OrderBy(x => x))
            await db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock({key})", cancellationToken);
    }

    private static IEnumerable<string> CreateScopes(Guid? userId, string? guestTokenHash)
    {
        if (userId.HasValue && userId.Value != Guid.Empty)
            yield return $"cart-user:{userId.Value:N}";
        if (!string.IsNullOrWhiteSpace(guestTokenHash))
            yield return $"cart-guest:{guestTokenHash}";
    }

    private static long ToAdvisoryLockKey(string scope) =>
        BitConverter.ToInt64(SHA256.HashData(Encoding.UTF8.GetBytes(scope)), 0);
}
