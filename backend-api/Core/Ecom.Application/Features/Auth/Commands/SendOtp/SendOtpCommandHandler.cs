using Ecom.Application.Common.Configuration;
using Ecom.Domain.Entities;
using Microsoft.Extensions.Options;

namespace Ecom.Application.Features.Auth.Commands.SendOtp;

[EnableUnitOfWork]
public class SendOtpCommandHandler(
    IUnitOfWork unitOfWork,
    ILogger<SendOtpCommandHandler> logger,
    IOptions<OtpSettings> otpOptions,
    IOtpSecurityService otpSecurity,
    ISmsSender smsSender,
    IAuthRateLimitService rateLimiter)
    : IRequestHandler<SendOtpCommand, TResult<SendOtpResult>>
{
    private readonly OtpSettings _otpSettings = otpOptions.Value;

    public async Task<TResult<SendOtpResult>> Handle(SendOtpCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
            return TResult<SendOtpResult>.Failure(MessageKey.PhoneNumberRequired, ErrorCodes.BAD_REQUEST);

        foreach (var policy in new[]
                 {
                     AuthRateLimitPolicyNames.OtpSendDestinationBurst,
                     AuthRateLimitPolicyNames.OtpSendDestinationDaily
                 })
        {
            var rateLimit = await rateLimiter.AcquireAsync(policy, request.PhoneNumber, cancellationToken);
            if (rateLimit.Status == AuthRateLimitStatus.Unavailable)
                return TResult<SendOtpResult>.Failure(MessageKey.AuthDependencyUnavailable, ErrorCodes.SERVICE_UNAVAILABLE);
            if (rateLimit.Status == AuthRateLimitStatus.Rejected)
                return TResult<SendOtpResult>.Failure(MessageKey.TooManyRequests, ErrorCodes.TOO_MANY_REQUESTS);
        }
        if (!smsSender.IsConfigured && !otpSecurity.IsDevelopmentTestAccount(request.PhoneNumber))
            return TResult<SendOtpResult>.Failure(MessageKey.AuthDependencyUnavailable, ErrorCodes.SERVICE_UNAVAILABLE);

        try
        {
            var user = await unitOfWork.Repository<User>()
                .FindOneAsync(filters: [u => u.PhoneNumber == request.PhoneNumber], includes: [u => u.Role!]);

            // Keep the public result neutral for missing or disabled accounts.
            if (user is null || user.Status == UserStatusEnum.Deactivated)
                return Accepted();

            var purpose = user.Status == UserStatusEnum.Pending
                ? OtpTokenTypeEnum.ActivateAccount
                : OtpTokenTypeEnum.Login;
            var now = DateTime.UtcNow;
            var existingOtp = await unitOfWork.Repository<OtpToken>().FindOneAsync(filters:
            [
                o => o.UserId == user.Id,
                o => o.OtpTokenType == purpose,
                o => !o.IsUsed
            ], orderBy: "CreatedAt desc");

            var lastSent = existingOtp?.UpdatedAt ?? existingOtp?.CreatedAt;
            if (lastSent.HasValue &&
                lastSent.Value.AddSeconds(_otpSettings.ResendCooldownSeconds) > now)
            {
                return Accepted();
            }

            var isDevelopmentTestAccount = otpSecurity.IsDevelopmentTestAccount(request.PhoneNumber);
            var otpCode = isDevelopmentTestAccount
                ? otpSecurity.DevelopmentOtp
                : otpSecurity.GenerateCode();
            var protectedCode = otpSecurity.Protect(user.Id, purpose, otpCode);

            if (existingOtp is not null)
            {
                existingOtp.UpdateNewCode(protectedCode, _otpSettings.ExpirationSeconds, _otpSettings.MaxAttempts);
                existingOtp.OtpTokenType = purpose;
                await unitOfWork.Repository<OtpToken>().UpdateAsync(existingOtp);
            }
            else
            {
                await unitOfWork.Repository<OtpToken>().InsertAsync(new OtpToken
                {
                    UserId = user.Id,
                    Code = protectedCode,
                    OtpTokenType = purpose,
                    PhoneNumber = request.PhoneNumber,
                    ExpiredAt = now.AddSeconds(_otpSettings.ExpirationSeconds),
                    MaxAttempts = _otpSettings.MaxAttempts,
                    IsUsed = false,
                    CreatedAt = now
                }, cancellationToken);
            }

            if (!isDevelopmentTestAccount)
            {
                await smsSender.SendAsync(
                    request.PhoneNumber,
                    otpCode,
                    Math.Max(1, (int)Math.Ceiling(_otpSettings.ExpirationSeconds / 60d)),
                    cancellationToken);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            logger.LogInformation("OTP request accepted. UserId: {UserId}, Purpose: {Purpose}", user.Id, purpose);

            return Accepted(otpSecurity.CanExposeDevelopmentOtp && isDevelopmentTestAccount ? otpCode : null);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning("OTP delivery dependency unavailable. Reason category: {Reason}", ex.GetType().Name);
            return TResult<SendOtpResult>.Failure(
                MessageKey.AuthDependencyUnavailable,
                ErrorCodes.SERVICE_UNAVAILABLE);
        }
        catch (Exception ex)
        {
            logger.LogError("OTP request failed. ExceptionType: {ExceptionType}", ex.GetType().Name);
            return TResult<SendOtpResult>.Failure(MessageKey.InternalError, ErrorCodes.SERVER_ERROR);
        }
    }

    private TResult<SendOtpResult> Accepted(string? developmentOtp = null) =>
        TResult<SendOtpResult>.Success(new SendOtpResult
        {
            ExpiresInSeconds = _otpSettings.ExpirationSeconds,
            CanResendAt = DateTime.UtcNow.AddSeconds(_otpSettings.ResendCooldownSeconds),
            OtpCode = developmentOtp,
            Message = MessageKey.AuthRequestAccepted,
            Status = "Accepted"
        });
}
