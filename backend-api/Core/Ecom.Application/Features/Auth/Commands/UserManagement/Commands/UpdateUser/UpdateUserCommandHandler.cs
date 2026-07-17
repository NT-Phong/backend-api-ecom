using Ecom.Domain.Entities;
using Ecom.Domain.Enums;
using DomainPermissions = global::Ecom.Domain.Constants.Permissions;

namespace Ecom.Application.Features.Auth.Commands.UserManagement.Commands.UpdateUser;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, TResult<UpdateUserResult>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public UpdateUserCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<TResult<UpdateUserResult>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        // 1. Get User
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (user == null)
            return TResult<UpdateUserResult>.Failure(MessageKey.UserNotFound);

        // 2. Get target role
        var targetRole = await _context.Roles.FindAsync(new object[] { request.RoleId }, cancellationToken);
        if (targetRole == null)
            return TResult<UpdateUserResult>.Failure(MessageKey.RoleNotFound);

        // 3. Authorization check
        var currentUser = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == _currentUser.UserId, cancellationToken);

        if (currentUser == null)
            return TResult<UpdateUserResult>.Failure(MessageKey.UserNotFound);

        // Only Admin role can update users
        if (currentUser.Role?.Code != DomainPermissions.Roles.Admin)
        {
            return TResult<UpdateUserResult>.Failure(MessageKey.InsufficientPermissions);
        }

        // 4. Update
        user.FullName = request.FullName;
        user.RoleId = request.RoleId;
        user.Status = request.IsActive ? UserStatusEnum.Active : UserStatusEnum.Deactivated;
        user.UpdatedAt = DateTime.UtcNow;

        var now = DateTime.UtcNow;

        // 5. Revoke refresh token
        var refreshTokens = await _context.JwtRefreshTokens
            .Where(j => j.UserId == user.Id && j.ExpiresAt > now && j.Status != JwtRefreshTokenStatusEnum.Revoked)
            .ToListAsync(cancellationToken);
            
        foreach (var refreshToken in refreshTokens)
        {
            refreshToken.Status = JwtRefreshTokenStatusEnum.Revoked;
            refreshToken.RevokedAt = now;
            refreshToken.RevokedReason = "Admin changed user role or status";
        }

        await _context.SaveChangesAsync(cancellationToken);

        return TResult<UpdateUserResult>.Success(new UpdateUserResult
        {
            Id = user.Id,
            FullName = user.FullName ?? string.Empty,
            RoleId = user.RoleId ?? Guid.Empty,
            RoleName = targetRole.Name,
            IsActive = user.Status == UserStatusEnum.Active
        });
    }
}
