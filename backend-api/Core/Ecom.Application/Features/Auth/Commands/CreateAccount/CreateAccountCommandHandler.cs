using Ecom.Application.Common.Configuration;
using Ecom.Domain.Entities;
using Microsoft.Extensions.Options;

namespace Ecom.Application.Features.Auth.Commands.CreateAccount;

[EnableUnitOfWork]
public class CreateAccountCommandHandler(
    IUnitOfWork unitOfWork,
    ILogger<CreateAccountCommandHandler> logger,
    IOptions<OtpSettings> otpOptions,
    IOtpSecurityService otpSecurity,
    ISmsSender smsSender,
    IAuthRateLimitService rateLimiter)
    : IRequestHandler<CreateAccountCommand, TResult<CreateAccountResult>>
{
    private readonly OtpSettings _otpSettings = otpOptions.Value;

    public async Task<TResult<CreateAccountResult>> Handle(
        CreateAccountCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
            return TResult<CreateAccountResult>.Failure(MessageKey.PhoneNumberRequired, ErrorCodes.BAD_REQUEST);

        var rateLimit = await rateLimiter.AcquireAsync(
            AuthRateLimitPolicyNames.RegisterDestinationDaily,
            request.PhoneNumber,
            cancellationToken);
        if (rateLimit.Status == AuthRateLimitStatus.Unavailable)
            return TResult<CreateAccountResult>.Failure(MessageKey.AuthDependencyUnavailable, ErrorCodes.SERVICE_UNAVAILABLE);
        if (rateLimit.Status == AuthRateLimitStatus.Rejected)
            return TResult<CreateAccountResult>.Failure(MessageKey.TooManyRequests, ErrorCodes.TOO_MANY_REQUESTS);
        if (!smsSender.IsConfigured && !otpSecurity.IsDevelopmentTestAccount(request.PhoneNumber))
            return TResult<CreateAccountResult>.Failure(MessageKey.AuthDependencyUnavailable, ErrorCodes.SERVICE_UNAVAILABLE);

        try
        {
            var existingUser = await unitOfWork.Repository<User>()
                .FindOneAsync(filters: [u => u.PhoneNumber == request.PhoneNumber]);

            // Registration is deliberately non-enumerating. It does not issue a login challenge
            // for an already existing account.
            if (existingUser is not null && !otpSecurity.IsDevelopmentTestAccount(request.PhoneNumber))
                return Accepted(null);

            var user = existingUser;
            if (user is null)
            {
                var userRole = await unitOfWork.Repository<Role>()
                    .FindOneAsync(filters: [r => r.Code == Permissions.Roles.User]);
                user = new User(request.PhoneNumber, userRole?.Id);
                await unitOfWork.Repository<User>().InsertAsync(user, cancellationToken);
            }

            var isDevelopmentTestAccount = otpSecurity.IsDevelopmentTestAccount(request.PhoneNumber);
            var otpCode = isDevelopmentTestAccount
                ? otpSecurity.DevelopmentOtp
                : otpSecurity.GenerateCode();
            var purpose = OtpTokenTypeEnum.ActivateAccount;

            await unitOfWork.Repository<OtpToken>().InsertAsync(new OtpToken
            {
                UserId = user.Id,
                PhoneNumber = user.PhoneNumber,
                OtpTokenType = purpose,
                Code = otpSecurity.Protect(user.Id, purpose, otpCode),
                ExpiredAt = DateTime.UtcNow.AddSeconds(_otpSettings.ExpirationSeconds),
                MaxAttempts = _otpSettings.MaxAttempts,
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            }, cancellationToken);

            if (!isDevelopmentTestAccount)
            {
                await smsSender.SendAsync(
                    request.PhoneNumber,
                    otpCode,
                    Math.Max(1, (int)Math.Ceiling(_otpSettings.ExpirationSeconds / 60d)),
                    cancellationToken);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Authentication registration request accepted. UserId: {UserId}", user.Id);

            return Accepted(otpSecurity.CanExposeDevelopmentOtp && isDevelopmentTestAccount ? otpCode : null);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning("Registration dependency unavailable. Trace only; reason category: {Reason}", ex.GetType().Name);
            return TResult<CreateAccountResult>.Failure(
                MessageKey.AuthDependencyUnavailable,
                ErrorCodes.SERVICE_UNAVAILABLE);
        }
        catch (Exception ex)
        {
            logger.LogError("Registration request failed. ExceptionType: {ExceptionType}", ex.GetType().Name);
            return TResult<CreateAccountResult>.Failure(MessageKey.InternalError, ErrorCodes.SERVER_ERROR);
        }
    }

    private TResult<CreateAccountResult> Accepted(string? developmentOtp) =>
        TResult<CreateAccountResult>.Success(new CreateAccountResult
        {
            UserId = Guid.Empty,
            Status = "Accepted",
            TestOtp = developmentOtp,
            ExpiresIn = _otpSettings.ExpirationSeconds,
            Message = MessageKey.AuthRequestAccepted
        });
}
