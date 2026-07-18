namespace Ecom.Infrastructure.Security;

public sealed class UnavailableAuthRateLimitService : IAuthRateLimitService
{
    public Task<AuthRateLimitResult> AcquireAsync(
        string policyName,
        string partitionValue,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(AuthRateLimitResult.Unavailable());
}
