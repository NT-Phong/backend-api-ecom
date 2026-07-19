using Ecom.Application.Features.Auth.Commands.SendOtp;
using Ecom.Domain.Extensions;

namespace Ecom.Application.Features.Auth.Commands.CreateAccount;

/// <summary>
/// Compatibility wrapper for clients that still call register before requesting an OTP.
/// The phone-first flow itself is owned by SendOtpCommandHandler.
/// </summary>
public sealed class CreateAccountCommandHandler(ISender sender)
    : IRequestHandler<CreateAccountCommand, TResult<CreateAccountResult>>
{
    public async Task<TResult<CreateAccountResult>> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        var otpResult = await sender.Send(new SendOtpCommand
        {
            PhoneNumber = request.PhoneNumber,
            LoginSource = "register-compatibility"
        }, cancellationToken);

        if (!otpResult.IsSuccess)
            return TResult<CreateAccountResult>.Failure(otpResult.Error ?? MessageKey.AuthenticationFailed, otpResult.ErrorCode);

        VietnamesePhoneNumber.TryNormalize(request.PhoneNumber, out var phoneNumber);
        return TResult<CreateAccountResult>.Success(new CreateAccountResult
        {
            PhoneNumber = phoneNumber,
            IsProfileCompleted = false,
            Status = "Accepted",
            TestOtp = otpResult.Data.OtpCode,
            ExpiresIn = otpResult.Data.ExpiresInSeconds,
            Message = otpResult.Data.Message
        });
    }
}
