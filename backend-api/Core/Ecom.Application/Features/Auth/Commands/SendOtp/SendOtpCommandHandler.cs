using Ecom.Application.Common.Configuration;
using Ecom.Domain.Entities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Ecom.Application.Features.Auth.Commands.SendOtp;

/// <summary>
/// Handler gửi OTP đến số điện thoại
/// </summary>
[EnableUnitOfWork]
public class SendOtpCommandHandler : IRequestHandler<SendOtpCommand, TResult<SendOtpResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SendOtpCommandHandler> _logger;
    private readonly OtpSettings _otpSettings;
    private readonly IHostEnvironment _env;

    public SendOtpCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<SendOtpCommandHandler> logger,
        IOptions<OtpSettings> otpSettings,
        IHostEnvironment env)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _otpSettings = otpSettings.Value;
        _env = env;
    }

    public async Task<TResult<SendOtpResult>> Handle(SendOtpCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            return TResult<SendOtpResult>.Failure(MessageKey.PhoneNumberRequired, ErrorCodes.BAD_REQUEST);
        }
        try
        {
            // 1. TÌM USER
            var user = await _unitOfWork.Repository<User>()
                .FindOneAsync(filters: [u => u.PhoneNumber == request.PhoneNumber], includes: [u => u.Role!]);

            if (user == null)
            {
                return TResult<SendOtpResult>.Failure(MessageKey.PhoneNumberNotFound, ErrorCodes.NOT_FOUND);
            }

            if (user.Status == UserStatusEnum.Deactivated)
            {
                return TResult<SendOtpResult>.Failure(MessageKey.UserAccountDisabled, ErrorCodes.FORBIDDEN);
            }

            bool isTestAccount = IsDevTestAccount(request.PhoneNumber, _env);

            string otpCode = isTestAccount
                ? _otpSettings.DefaultOtp
                : GenerateOtpCode(_otpSettings.OtpLength);

            var inferredType = user.Status == UserStatusEnum.Pending
                ? OtpTokenTypeEnum.ActivateAccount
                : OtpTokenTypeEnum.Login;
            var now = DateTime.UtcNow;

            // 2. Nếu otp cũ còn hạn thì -> trả về luôn
            var existingOtp = await _unitOfWork.Repository<OtpToken>().FindOneAsync(filters: [
                o => o.UserId == user.Id
            ]);

            if (user.Status == UserStatusEnum.Pending && existingOtp != null && !existingOtp.IsUsed && existingOtp.ExpiredAt > now)
            {
                return TResult<SendOtpResult>.Success(new SendOtpResult
                {
                    Status = "ResendCooldown",
                    IsPending = true,
                    OtpCode = !_env.IsProduction() ? existingOtp.Code : null,
                    ExpiresInSeconds = (int)(existingOtp.ExpiredAt - now).TotalSeconds,
                    Message = MessageKey.UserNotActive
                });
            }

            var lastTimeSent = existingOtp?.UpdatedAt ?? existingOtp?.CreatedAt;
            if (lastTimeSent.HasValue && lastTimeSent.Value.AddSeconds(_otpSettings.ResendCooldownSeconds) > now)
            {
                var wait = (int)(lastTimeSent.Value.AddSeconds(_otpSettings.ResendCooldownSeconds) - now).TotalSeconds;
                var errorMessage = string.Format(MessageKey.OtpResendWait, wait);
                return TResult<SendOtpResult>.Failure(errorMessage, ErrorCodes.BAD_REQUEST);
            }

            // 3. SINH MÃ MỚI & XỬ LÝ DATABASE (CẬP NHẬT HOẶC THÊM MỚI)
            int expirationSeconds = isTestAccount ? (int)TimeSpan.FromDays(365).TotalSeconds : _otpSettings.ExpirationSeconds;
            int maxAttempts = isTestAccount ? 999 : _otpSettings.MaxAttempts;

            if (existingOtp != null)
            {
                existingOtp.UpdateNewCode(otpCode, expirationSeconds, maxAttempts);
                existingOtp.OtpTokenType = inferredType;
                await _unitOfWork.Repository<OtpToken>().UpdateAsync(existingOtp);
            }
            else
            {
                var newOtp = new OtpToken
                {
                    UserId = user.Id,
                    Code = otpCode,
                    OtpTokenType = inferredType,
                    PhoneNumber = request.PhoneNumber,
                    ExpiredAt = DateTime.UtcNow.AddSeconds(expirationSeconds),
                    MaxAttempts = maxAttempts,
                    IsUsed = false,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.Repository<OtpToken>().InsertAsync(newOtp, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Log the OTP code for local environment verification
            _logger.LogInformation("--- [OTP VERIFICATION CODE] For PhoneNumber {Phone}: {OtpCode} ---", request.PhoneNumber, otpCode);

            // 5. TRẢ VỀ KẾT QUẢ
            bool isPending = user.Status == UserStatusEnum.Pending && !isTestAccount;
            var result = new SendOtpResult
            {
                ExpiresInSeconds = _otpSettings.ExpirationSeconds,
                CanResendAt = now.AddSeconds(_otpSettings.ResendCooldownSeconds),
                IsPending = isPending,
                Status = isPending ? "Unverified" : "Completed",
                OtpCode = otpCode,
                Message = isPending
                    ? MessageKey.UserNotActive
                    : MessageKey.OtpSentSuccess
            };

            return TResult<SendOtpResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending OTP to PhoneNumber: {PhoneNumber}", request.PhoneNumber);
            return TResult<SendOtpResult>.Failure(MessageKey.InternalError, ErrorCodes.SERVER_ERROR);
        }
    }

    private static bool IsDevTestAccount(string phoneNumber, IHostEnvironment env)
    {
        return (env.IsDevelopment() || env.IsStaging()) &&
               TestAccounts.All.Contains(phoneNumber);
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

