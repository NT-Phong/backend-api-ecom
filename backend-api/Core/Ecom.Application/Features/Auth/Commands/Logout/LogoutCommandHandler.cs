using Ecom.Application.Common.Interfaces;
using Ecom.Application.Common.Models;
using Ecom.Domain.Entities;
using Ecom.Domain.Interfaces.Repositories;
using MediatR;

namespace Ecom.Application.Features.Auth.Commands.Logout;

public class LogoutCommandHandler(
    IJwtTokenService jwtTokenService,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IAuthTokenProtector tokenProtector
) : IRequestHandler<LogoutCommand, TResult>
{
    public async Task<TResult> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var protectedToken = string.IsNullOrWhiteSpace(request.RefreshToken) ? string.Empty : tokenProtector.Protect(request.RefreshToken);
        var refreshToken = await unitOfWork.Repository<JwtRefreshToken>()
            .FindOneAsync(filters: [t => t.Token == protectedToken || t.Token == request.RefreshToken]);
        var sessionRefreshToken = string.IsNullOrEmpty(protectedToken) ? null :
            await unitOfWork.Repository<SessionRefreshToken>().FindOneAsync(filters: [t => t.TokenHash == protectedToken]);
        var tokenSession = sessionRefreshToken is null ? null :
            await unitOfWork.Repository<UserSession>().FindByIdAsync(sessionRefreshToken.SessionId);

        var targetUserId = tokenSession?.UserId ?? (refreshToken is { IsActive: true }
            ? refreshToken.UserId
            : (currentUser.IsAuthenticated && currentUser.UserId != Guid.Empty
                ? currentUser.UserId
                : Guid.Empty));
        var targetSessionId = request.SessionId ?? tokenSession?.Id ?? currentUser.SessionId;

        await unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            if (!string.IsNullOrWhiteSpace(request.RefreshToken))
                await jwtTokenService.RevokeRefreshToken(request.RefreshToken, cancellationToken);

        if (targetUserId != Guid.Empty)
        {
            var now = DateTime.UtcNow;
            var sessions = await unitOfWork.Repository<UserSession>().FindAsync(filters:
                [s => s.UserId == targetUserId && s.RevokedAt == null && (request.LogoutAllDevices || s.Id == targetSessionId)]);
            foreach (var session in sessions)
            {
                session.Revoke(now, request.LogoutAllDevices ? "LogoutAll" : "Logout");
                await unitOfWork.Repository<UserSession>().UpdateAsync(session);
                var sessionTokens = await unitOfWork.Repository<SessionRefreshToken>().FindAsync(filters: [t => t.SessionId == session.Id && t.RevokedAt == null]);
                foreach (var sessionToken in sessionTokens) { sessionToken.Revoke(now); await unitOfWork.Repository<SessionRefreshToken>().UpdateAsync(sessionToken); }
                await unitOfWork.Repository<SecurityEvent>().InsertAsync(new SecurityEvent(targetUserId, session.Id,
                    "LogoutSucceeded", SecurityRiskLevel.Low, true, "not-captured", "not-captured", null, now), cancellationToken);
            }
            if (request.LogoutAllDevices)
            {
                var user = await unitOfWork.Repository<User>().FindByIdAsync(targetUserId);
                if (user is not null) { user.RotateSecurityStamp(); await unitOfWork.Repository<User>().UpdateAsync(user); }
                var refreshTokens = await unitOfWork.Repository<JwtRefreshToken>()
                    .FindAsync(filters: [t => t.UserId == targetUserId]);
                foreach (var token in refreshTokens.Where(t => t.IsActive))
                {
                    token.RevokedAt = DateTime.UtcNow;
                    token.Status = JwtRefreshTokenStatusEnum.Revoked;
                    token.RevokedReason = "LogoutAll";
                    token.UpdatedAt = DateTime.UtcNow;
                    await unitOfWork.Repository<JwtRefreshToken>().UpdateAsync(token);
                }

                await unitOfWork.Repository<SecurityEvent>().InsertAsync(new SecurityEvent(targetUserId, null,
                    "LogoutAllSucceeded", SecurityRiskLevel.Low, true, "not-captured", "not-captured", null, now), cancellationToken);

                var tokens = await unitOfWork.Repository<UserDeviceToken>()
                    .FindAsync(
                        filters:
                        [
                            t => t.UserId == targetUserId && t.IsActive && !t.IsDeleted
                        ]);

                foreach (var token in tokens) token.Deactivate();
                
            }
            else if (!string.IsNullOrWhiteSpace(request.FcmToken))
            {
                var token = await unitOfWork.Repository<UserDeviceToken>()
                    .FindOneAsync(
                        filters:
                        [
                            t => t.UserId == targetUserId
                                 && t.FcmToken == request.FcmToken
                                 && t.IsActive
                                 && !t.IsDeleted
                        ]);

                if (token is not null)
                {
                    token.Deactivate();
                }
            }
        }
        }, cancellationToken);

        return TResult.Success();
    }
}
