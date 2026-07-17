using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Auth.Commands.CompleteProfile;

/// <summary>
/// Handler xử lý hoàn thiện thông tin người dùng sau khi xác thực OTP thành công
/// Template pattern: Find Entity -> Call Domain Method -> Save -> Return
/// </summary>
[EnableUnitOfWork]
public class CompleteProfileCommandHandler : IRequestHandler<CompleteProfileCommand, TResult<CompleteProfileResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<CompleteProfileCommandHandler> _logger;

    public CompleteProfileCommandHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser, ILogger<CompleteProfileCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<TResult<CompleteProfileResult>> Handle(CompleteProfileCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId;
        if (currentUserId == Guid.Empty)
        {
            _logger.LogWarning("Unauthorized profile completion attempt for User: {UserId}", request.UserId);
            return TResult<CompleteProfileResult>.Failure(MessageKey.Unauthorized, ErrorCodes.UNAUTHORIZED);
        }
        _logger.LogInformation("Completing profile for User: {UserId}", currentUserId);

        try
        {
            // 1. Tìm người dùng theo UserId
            var user = await _unitOfWork.Repository<User>().FindByIdAsync(currentUserId);

            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found", currentUserId);
                return TResult<CompleteProfileResult>.Failure(MessageKey.UserNotFound, ErrorCodes.NOT_FOUND);
            }

            // 2. Gọi hàm nghiệp vụ trong Entity
            // Hàm này sẽ gán FullName, Email, Address và IsProfileCompleted = true
            user.CompleteProfile(
                fullName: request.FullName.Trim(),
                email: string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
                address: request.Address?.Trim(),
                avatarId: request.AvatarId
            );

            // 3. Cập nhật vào database
            await _unitOfWork.Repository<User>().UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 4. Trả về dữ liệu đã hoàn thiện
            var result = new CompleteProfileResult
            {
                UserId = user.Id,
                FullName = user.FullName ?? string.Empty,
                Email = user.Email,
                Address = user.Address,
                AvatarId = user.AvatarId,
                IsProfileCompleted = user.IsProfileCompleted
            };

            _logger.LogInformation("Profile completed successfully for User: {UserId}", currentUserId);

            return TResult<CompleteProfileResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while completing profile for User: {UserId}", currentUserId);
            return TResult<CompleteProfileResult>.Failure(MessageKey.InternalError, ErrorCodes.SERVER_ERROR);
        }
    }
}
