using Ecom.Application.Common.Configuration;

namespace Ecom.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler(
    IJwtTokenService jwtTokenService,
    IAuthRateLimitService rateLimiter,
    IDistributedLockService distributedLock,
    IAuthTokenProtector tokenProtector)
    : IRequestHandler<RefreshTokenCommand, TResult<RefreshTokenResult>>
{
    public async Task<TResult<RefreshTokenResult>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        var rateLimit = await rateLimiter.AcquireAsync(
            AuthRateLimitPolicyNames.RefreshSession,
            request.RefreshToken,
            cancellationToken);
        if (rateLimit.Status == AuthRateLimitStatus.Unavailable)
            return TResult<RefreshTokenResult>.Failure(MessageKey.AuthDependencyUnavailable, ErrorCodes.SERVICE_UNAVAILABLE);
        if (rateLimit.Status == AuthRateLimitStatus.Rejected)
            return TResult<RefreshTokenResult>.Failure(MessageKey.TooManyRequests, ErrorCodes.TOO_MANY_REQUESTS);

        await using var refreshLock = await distributedLock.TryAcquireAsync(
            $"auth:refresh:{tokenProtector.Protect(request.RefreshToken)}",
            TimeSpan.FromSeconds(1),
            cancellationToken);
        if (refreshLock is null)
            return TResult<RefreshTokenResult>.Failure(MessageKey.TooManyRequests, ErrorCodes.TOO_MANY_REQUESTS);

        return await jwtTokenService.RefreshJwtToken(request.RefreshToken, cancellationToken);
    }
}
