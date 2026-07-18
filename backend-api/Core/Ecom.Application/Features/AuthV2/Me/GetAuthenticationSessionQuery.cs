using Ecom.Domain.Entities;

namespace Ecom.Application.Features.AuthV2.Me;

public sealed record GetAuthenticationSessionQuery : IRequest<TResult<GetAuthenticationSessionResult>>;
public sealed record GetAuthenticationSessionResult(Guid UserId, Guid SessionId, string? Username,
    string? Role, IReadOnlyCollection<string> Policies, DateTime IdleExpiresAt, DateTime AbsoluteExpiresAt);

public sealed class GetAuthenticationSessionQueryHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    : IRequestHandler<GetAuthenticationSessionQuery, TResult<GetAuthenticationSessionResult>>
{
    public async Task<TResult<GetAuthenticationSessionResult>> Handle(GetAuthenticationSessionQuery request, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId == Guid.Empty || currentUser.SessionId == Guid.Empty)
            return TResult<GetAuthenticationSessionResult>.Failure(MessageKey.Unauthorized, ErrorCodes.UNAUTHORIZED);
        var user = await unitOfWork.Repository<User>().FindByIdAsync(currentUser.UserId, x => x.Role!);
        var session = await unitOfWork.Repository<UserSession>().FindByIdAsync(currentUser.SessionId);
        var now = DateTime.UtcNow;
        if (user is null || session is null || session.UserId != user.Id || user.Status != UserStatusEnum.Active ||
            currentUser.SecurityStamp != user.SecurityStamp || !session.IsActive(now, user.SecurityStamp))
            return TResult<GetAuthenticationSessionResult>.Failure(MessageKey.Unauthorized, ErrorCodes.UNAUTHORIZED);
        return TResult<GetAuthenticationSessionResult>.Success(new(user.Id, session.Id, user.Username,
            user.Role?.Code, currentUser.Policies.ToArray(), session.IdleExpiresAt, session.AbsoluteExpiresAt));
    }
}
