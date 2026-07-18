using Ecom.Application.Common.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace Ecom.Infrastructure.Security;

public sealed class DistributedAuthRateLimitService(
    IAuthRateLimitCounterStore store,
    IOptions<AuthRateLimitOptions> options) : IAuthRateLimitService
{
    private readonly AuthRateLimitOptions _options = options.Value;

    public async Task<AuthRateLimitResult> AcquireAsync(
        string policyName,
        string partitionValue,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(partitionValue))
            return AuthRateLimitResult.Unavailable();

        var rule = _options.GetDistributedPolicy(policyName);
        if (rule.PermitLimit <= 0 || rule.WindowSeconds <= 0)
            return AuthRateLimitResult.Unavailable();

        var fingerprint = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(partitionValue)));
        var key = $"{_options.RedisKeyPrefix}:{policyName}:{fingerprint}";
        var counter = await store.IncrementAsync(
            key,
            TimeSpan.FromSeconds(rule.WindowSeconds),
            cancellationToken);

        if (counter is null)
            return AuthRateLimitResult.Unavailable();

        return counter.Count <= rule.PermitLimit
            ? AuthRateLimitResult.Allowed()
            : AuthRateLimitResult.Rejected(counter.TimeToLive);
    }
}
