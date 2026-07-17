using Ecom.Application.Common.Interfaces;
using Ecom.Application.Common.Models;
using Ecom.Domain.Entities;
using Ecom.Domain.Interfaces.Repositories;
using MediatR;

namespace Ecom.Application.Features.Auth.Commands.Logout;

public class LogoutCommandHandler(
    IJwtTokenService jwtTokenService,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser
) : IRequestHandler<LogoutCommand, TResult>
{
    public async Task<TResult> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var refreshToken = await unitOfWork.Repository<JwtRefreshToken>()
            .FindOneAsync(filters: [t => t.Token == request.RefreshToken]);

        var targetUserId = currentUser.IsAuthenticated && currentUser.UserId != Guid.Empty
            ? currentUser.UserId
            : (refreshToken is { IsActive: true } ? refreshToken.UserId : Guid.Empty);

        await jwtTokenService.RevokeRefreshToken(request.RefreshToken, cancellationToken);

        if (targetUserId != Guid.Empty)
        {
            if (request.LogoutAllDevices)
            {
                var tokens = await unitOfWork.Repository<UserDeviceToken>()
                    .FindAsync(
                        filters:
                        [
                            t => t.UserId == targetUserId && t.IsActive && !t.IsDeleted
                        ]);

                foreach (var token in tokens) token.Deactivate();
                
                if (tokens.Any()) await unitOfWork.SaveChangesAsync(cancellationToken);
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
                    await unitOfWork.SaveChangesAsync(cancellationToken);
                }
            }
        }

        return TResult.Success();
    }
}
