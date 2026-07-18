using Ecom.Application.Common.Interfaces;
using Ecom.Domain.Entities;
using Ecom.Domain.Enums;
using System.Security.Cryptography;

namespace Ecom.Application.Common.Services;

public sealed class AuthenticationSessionEngine(IUnitOfWork unitOfWork, IJwtTokenService jwt,
    IAuthTokenProtector protector, IUserAuthorizationSnapshotService authorization,
    Microsoft.Extensions.Options.IOptions<Ecom.Application.Common.Configuration.PasswordAuthenticationV2Options> options) : IAuthenticationSessionEngine
{
    public async Task<TResult<AuthenticationSessionResult>> CreateAsync(VerifiedAuthenticationContext context, CancellationToken ct)
    {
        var user = await unitOfWork.Repository<User>().FindByIdAsync(context.UserId, x => x.Role!);
        if (user is null || user.Status != UserStatusEnum.Active)
            return TResult<AuthenticationSessionResult>.Failure(MessageKey.AuthenticationFailed, ErrorCodes.UNAUTHORIZED);
        var now = DateTime.UtcNow;
        var role = user.Role?.Code?.ToUpperInvariant();
        var (idle, absolute) = role switch
        {
            "ADMIN" or "SYSTEMADMIN" => (TimeSpan.FromMinutes(15), TimeSpan.FromHours(4)),
            "STAFF" => (TimeSpan.FromMinutes(30), TimeSpan.FromHours(8)),
            "SELLER" or "ORGANIZATION" => (TimeSpan.FromHours(8), TimeSpan.FromDays(7)),
            _ when context.RememberMe => (TimeSpan.FromDays(7), TimeSpan.FromDays(30)),
            _ => (TimeSpan.FromHours(24), TimeSpan.FromDays(7))
        };
        var session = new UserSession(user.Id, context.ClientType, context.Method,
            AuthenticationStrength.SingleFactor, user.SecurityStamp, context.DeviceId, now, now.Add(idle), now.Add(absolute));
        await unitOfWork.Repository<UserSession>().InsertAsync(session, ct);
        string? access = null, refresh = null; var refreshExpiresAt = now;
        if (context.ClientType == SessionClientType.Mobile)
        {
            refresh = Base64Url(RandomNumberGenerator.GetBytes(32));
            refreshExpiresAt = now.AddDays(options.Value.RefreshTokenDays);
            if (refreshExpiresAt > now.Add(absolute)) refreshExpiresAt = now.Add(absolute);
            await unitOfWork.Repository<SessionRefreshToken>().InsertAsync(new SessionRefreshToken(session.Id,
                Guid.NewGuid(), protector.Protect(refresh), now, refreshExpiresAt), ct);
            var policies = await authorization.ResolvePoliciesAsync(user, ct);
            access = jwt.GenerateAccessToken(user, policies, session.Id, user.SecurityStamp);
        }
        await unitOfWork.Repository<SecurityEvent>().InsertAsync(new SecurityEvent(user.Id, session.Id,
            "LoginSucceeded", SecurityRiskLevel.Low, true, context.IpFingerprint, context.UserAgentSummary, null, now), ct);
        return TResult<AuthenticationSessionResult>.Success(new(session.Id, access, refresh,
            jwt.GetAccessTokenExpiration(), refreshExpiresAt, now.Add(idle), now.Add(absolute)));
    }
    private static string Base64Url(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+','-').Replace('/','_');
}
