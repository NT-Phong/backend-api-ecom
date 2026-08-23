using Ecom.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ecom.Application.Features.Auth.Commands.UpdateUserRole;

[EnableUnitOfWork]
public class UpdateUserRoleCommandHandler : IRequestHandler<UpdateUserRoleCommand, TResult<UpdateUserRoleResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public UpdateUserRoleCommandHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<TResult<UpdateUserRoleResult>> Handle(UpdateUserRoleCommand request, CancellationToken cancellationToken)
    {
        // 1. Kiểm tra quyền Admin (Chỉ SystemAdmin mới được đổi quyền người khác)
        if (_currentUser.Role != Permissions.Roles.Admin)
        {
            return TResult<UpdateUserRoleResult>.Failure(MessageKey.Forbidden, ErrorCodes.FORBIDDEN);
        }

        // 2. Chặn việc tự đổi quyền chính mình
        if (request.TargetUserId == _currentUser.UserId)
        {
            return TResult<UpdateUserRoleResult>.Failure(MessageKey.CannotUpdateOwnRole, ErrorCodes.BAD_REQUEST);
        }

        // 3. Tìm User (Lấy từ TargetUserId trên URL)
        var targetUser = await _unitOfWork.Repository<User>().FindByIdAsync(request.TargetUserId);
        if (targetUser == null)
        {
            return TResult<UpdateUserRoleResult>.Failure(MessageKey.UserNotFound, ErrorCodes.NOT_FOUND);
        }

        // 4. Tìm Role mới (Lấy từ NewRoleId trong Body)
        var newRole = await _unitOfWork.Repository<Role>().FindByIdAsync(request.NewRoleId);
        if (newRole == null)
        {
            return TResult<UpdateUserRoleResult>.Failure(MessageKey.RoleNotFound, ErrorCodes.NOT_FOUND);
        }

        // 5. Cập nhật
        var now = DateTime.UtcNow;
        targetUser.RoleId = newRole.Id;
        targetUser.UpdatedAt = now;

        // 6. Revoke refresh token
        var refreshTokens = await _unitOfWork.Repository<JwtRefreshToken>().FindAsync([j => j.UserId == targetUser.Id && j.ExpiresAt > DateTime.UtcNow && j.Status != JwtRefreshTokenStatusEnum.Revoked]);
        foreach (var refreshToken in refreshTokens)
        {
            refreshToken.Status = JwtRefreshTokenStatusEnum.Revoked;
            refreshToken.RevokedAt = now;
            refreshToken.RevokedReason = "Admin changed user role";
            
            await _unitOfWork.Repository<JwtRefreshToken>().UpdateAsync(refreshToken);
        }

        await _unitOfWork.Repository<User>().UpdateAsync(targetUser);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 7. Trả về kết quả kèm thông báo thành công
        return TResult<UpdateUserRoleResult>.Success(new UpdateUserRoleResult
        {
            UserId = targetUser.Id,
            PhoneNumber = targetUser.PhoneNumber ?? string.Empty,
            FullName = targetUser.FullName,
            NewRoleId = newRole.Id,
            NewRoleName = newRole.Name,
            NewRoleCode = newRole.Code,
            UpdatedAt = now
        });
    }
}
