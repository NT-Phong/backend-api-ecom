using Ecom.Application.Common.Configuration;
using Ecom.Domain.Entities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Ecom.Application.Features.Auth.Commands.CreateAccount;

/// <summary>
/// Handler để đăng ký tài khoản mới bằng số điện thoại
/// User sau khi đăng ký sẽ ở trạng thái Pending, cần xác thực OTP để Active
/// </summary>
[EnableUnitOfWork]
public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, TResult<CreateAccountResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateAccountCommandHandler> _logger;
    private readonly IHostEnvironment _env;
    private readonly OtpSettings _otpSettings;

    public CreateAccountCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<CreateAccountCommandHandler> logger,
        IHostEnvironment env,
        IOptions<OtpSettings> otpOptions)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _env = env;
        _otpSettings = otpOptions.Value;
    }

    public async Task<TResult<CreateAccountResult>> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            return TResult<CreateAccountResult>.Failure(MessageKey.PhoneNumberRequired, ErrorCodes.BAD_REQUEST);
        }

        _logger.LogInformation("Creating new account for PhoneNumber: {PhoneNumber}", request.PhoneNumber);
        try
        {
            var existingUserByPhone = await _unitOfWork.Repository<User>()
                    .FindOneAsync(filters: [u => u.PhoneNumber == request.PhoneNumber]);

            if (existingUserByPhone != null && request.PhoneNumber != TestAccounts.Manager)
            {
                _logger.LogWarning("PhoneNumber {PhoneNumber} already exists", request.PhoneNumber);
                return TResult<CreateAccountResult>.Failure(MessageKey.PhoneNumberAlreadyExists, ErrorCodes.ALREADY_EXISTS);
            }

            User user;

            if (existingUserByPhone != null && request.PhoneNumber == TestAccounts.Manager)
            {
                user = existingUserByPhone;
            }
            else
            {
                var userRole = await _unitOfWork.Repository<Role>()
                    .FindOneAsync(filters: [r => r.Code == Permissions.Roles.User]);

                user = new User(request.PhoneNumber, userRole?.Id);
                await _unitOfWork.Repository<User>().InsertAsync(user, cancellationToken);
            }

            string otpCode = request.PhoneNumber == TestAccounts.Manager
                 ? "0000"
                 : GenerateOtpCode(_otpSettings.OtpLength);

            var expiryTime = DateTime.UtcNow.AddSeconds(_otpSettings.ExpirationSeconds);

            var otpEntry = new OtpToken
            {
                UserId = user.Id,
                PhoneNumber = user.PhoneNumber,
                OtpTokenType = OtpTokenTypeEnum.ActivateAccount,
                Code = otpCode,
                ExpiredAt = expiryTime,
                MaxAttempts = _otpSettings.MaxAttempts,
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Repository<OtpToken>().InsertAsync(otpEntry, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Account created successfully with Id: {UserId}, Status: Pending", user.Id);

            return TResult<CreateAccountResult>.Success(new CreateAccountResult
            {
                UserId = user.Id,
                PhoneNumber = user.PhoneNumber,
                Status = user.Status.ToString(),
                TestOtp = _env.IsProduction() ? null : otpCode,
                ExpiresIn = _otpSettings.ExpirationSeconds,
                Message = MessageKey.RegisterSuccess
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating account for PhoneNumber: {PhoneNumber}", request.PhoneNumber);
            return TResult<CreateAccountResult>.Failure(MessageKey.InternalError, ErrorCodes.SERVER_ERROR);
        }
    }

    private static string GenerateOtpCode(int length)
    {
        var random = new Random();
        var otp = string.Empty;
        for (int i = 0; i < length; i++)
        {
            otp += random.Next(0, 10).ToString();
        }
        return otp;
    }
}

