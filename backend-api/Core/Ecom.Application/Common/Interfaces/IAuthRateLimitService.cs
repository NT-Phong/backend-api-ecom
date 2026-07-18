namespace Ecom.Application.Common.Interfaces;

public enum AuthRateLimitStatus
{
    Allowed,
    Rejected,
    Unavailable
}

public sealed record AuthRateLimitResult(AuthRateLimitStatus Status, TimeSpan? RetryAfter = null)
{
    public static AuthRateLimitResult Allowed() => new(AuthRateLimitStatus.Allowed);
    public static AuthRateLimitResult Rejected(TimeSpan retryAfter) => new(AuthRateLimitStatus.Rejected, retryAfter);
    public static AuthRateLimitResult Unavailable() => new(AuthRateLimitStatus.Unavailable);
}

public interface IAuthRateLimitService
{
    Task<AuthRateLimitResult> AcquireAsync(
        string policyName,
        string partitionValue,
        CancellationToken cancellationToken = default);
}
