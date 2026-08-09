using Ecom.Application.Common.Configuration;
using Ecom.Domain.Entities;
using Ecom.Domain.Extensions;
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
        if (!VietnamesePhoneNumber.TryNormalize(request.PhoneNumber, out var phoneNumber))
            return TResult<SendOtpResult>.Failure(MessageKey.PhoneNumberRequired, ErrorCodes.BAD_REQUEST);

        foreach (var policy in new[]
                 {
                     AuthRateLimitPolicyNames.OtpSendDestinationBurst,
                     AuthRateLimitPolicyNames.OtpSendDestinationDaily
                 })
        {
            var rateLimit = await rateLimiter.AcquireAsync(policy, phoneNumber, cancellationToken);
            if (rateLimit.Status == AuthRateLimitStatus.Unavailable)
                return TResult<SendOtpResult>.Failure(MessageKey.AuthDependencyUnavailable, ErrorCodes.SERVICE_UNAVAILABLE);
            if (rateLimit.Status == AuthRateLimitStatus.Rejected)
                return TResult<SendOtpResult>.Failure(MessageKey.TooManyRequests, ErrorCodes.TOO_MANY_REQUESTS);
        }
        var testOtp = otpSecurity.GetTestOtp(phoneNumber, request.ControlledTestBypassKey);
        if (!smsSender.IsConfigured && testOtp is null)
            return TResult<SendOtpResult>.Failure(MessageKey.AuthDependencyUnavailable, ErrorCodes.SERVICE_UNAVAILABLE);

        try
        {
            var legacyInternationalPhoneNumber = "84" + phoneNumber[1..];
            var user = await unitOfWork.Repository<User>()
                .FindOneAsync(
                    filters: [u => u.NormalizedPhoneNumber == phoneNumber || u.NormalizedPhoneNumber == legacyInternationalPhoneNumber],
                    includes: [u => u.Role!]);

            // A new number begins a pending registration. The public response remains neutral.
            if (user is null)
            {
                var userRole = await unitOfWork.Repository<Role>()
                    .FindOneAsync(filters: [r => r.Code == Permissions.Roles.User]);
                user = new User(phoneNumber, userRole?.Id);
                await unitOfWork.Repository<User>().InsertAsync(user, cancellationToken);
            }

            // Keep the public result neutral for disabled accounts.
            if (user.Status == UserStatusEnum.Deactivated)
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

            // The controlled test bypass is authorized by a separate request header on
            // both OTP endpoints. Do not persist its predictable code as an OtpToken.
            if (testOtp is not null)
            {
                await unitOfWork.SaveChangesAsync(cancellationToken);
                logger.LogInformation("Controlled OTP test bypass accepted. UserId: {UserId}", user.Id);
                return Accepted();
            }

            var otpCode = otpSecurity.GenerateCode();
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
                    PhoneNumber = phoneNumber,
                    ExpiredAt = now.AddSeconds(_otpSettings.ExpirationSeconds),
                    MaxAttempts = _otpSettings.MaxAttempts,
                    IsUsed = false,
                    CreatedAt = now
                }, cancellationToken);
            }

            if (testOtp is null)
            {
                await smsSender.SendAsync(
                    phoneNumber,
                    otpCode,
                    Math.Max(1, (int)Math.Ceiling(_otpSettings.ExpirationSeconds / 60d)),
                    cancellationToken);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            logger.LogInformation("OTP request accepted. UserId: {UserId}, Purpose: {Purpose}", user.Id, purpose);

            return Accepted(otpSecurity.CanExposeDevelopmentOtp && otpSecurity.IsDevelopmentTestAccount(phoneNumber) ? otpCode : null);
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
