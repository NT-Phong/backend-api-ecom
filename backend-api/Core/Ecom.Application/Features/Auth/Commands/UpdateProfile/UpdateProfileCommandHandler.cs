using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Auth.Commands.UpdateProfile;

[EnableUnitOfWork]
public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, TResult<UpdateProfileResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<UpdateProfileCommandHandler> _logger;

    public UpdateProfileCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        ILogger<UpdateProfileCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<TResult<UpdateProfileResult>> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId;
        if (currentUserId == Guid.Empty)
        {
            return TResult<UpdateProfileResult>.Failure(MessageKey.Unauthorized, ErrorCodes.UNAUTHORIZED);
        }

        try
        {
            // 1. Tìm người dùng
            var user = await _unitOfWork.Repository<User>().FindByIdAsync(currentUserId);
            if (user == null)
            {
                return TResult<UpdateProfileResult>.Failure(MessageKey.UserNotFound, ErrorCodes.NOT_FOUND);
            }
            // 2. Cập nhật thông tin
            if (!string.IsNullOrWhiteSpace(request.FullName))
                user.FullName = request.FullName.Trim();
            // 3. Kiểm tra email có bị trùng không
            if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != user.Email)
            {
                var emailExists = await _unitOfWork.Repository<User>().AnyAsync(filters: [u => u.Email == request.Email && u.Id != currentUserId]);
                if (emailExists)
                {
                    return TResult<UpdateProfileResult>.Failure(MessageKey.UserEmailAlreadyExists, ErrorCodes.ALREADY_EXISTS);
                }
                user.SetEmail(request.Email);
            }

            if (request.Address != null)
                user.Address = request.Address.Trim();

            if (request.AvatarId.HasValue && request.AvatarId != Guid.Empty)
                user.AvatarId = request.AvatarId;

            user.UpdatedAt = DateTime.UtcNow;

            // 4. Lưu vào Database
            await _unitOfWork.Repository<User>().UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Profile updated successfully for User: {UserId}", currentUserId);

            return TResult<UpdateProfileResult>.Success(new UpdateProfileResult
            {
                UserId = user.Id,
                PhoneNumber = user.PhoneNumber,
                FullName = user.FullName ?? string.Empty,
                Email = user.Email,
                Address = user.Address,
                AvatarId = user.AvatarId,
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile for User: {UserId}", currentUserId);
            return TResult<UpdateProfileResult>.Failure(MessageKey.UserProfileUpdateFailed, ErrorCodes.SERVER_ERROR);
        }
    }
}
