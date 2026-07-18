using Ecom.Domain.Enums;

namespace Ecom.Application.Common.Interfaces;

public sealed record VerifiedAuthenticationContext(Guid UserId, AuthenticationMethod Method,
    SessionClientType ClientType, string? DeviceId, bool RememberMe, string IpFingerprint, string UserAgentSummary);
public sealed record AuthenticationSessionResult(Guid SessionId, string? AccessToken, string? RefreshToken,
    DateTime AccessTokenExpiresAt, DateTime RefreshTokenExpiresAt, DateTime IdleExpiresAt, DateTime AbsoluteExpiresAt);
public interface IAuthenticationSessionEngine
{
    Task<TResult<AuthenticationSessionResult>> CreateAsync(VerifiedAuthenticationContext context, CancellationToken ct);
}
