namespace Ecom.Application.Features.Auth.Commands.VerifyOtp;

public class VerifyOtpCommandValidator : AbstractValidator<VerifyOtpCommand>
{

    public VerifyOtpCommandValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;
        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage(MessageKey.PhoneNumberRequired)

            .Must(value => Ecom.Domain.Extensions.VietnamesePhoneNumber.TryNormalize(value, out _))
            .WithMessage(MessageKey.PhoneNumberInvalid);

        RuleFor(x => x.OtpCode)
            .NotEmpty().WithMessage("Mã OTP không được để trống")
            .Matches(@"^\d{4}$").WithMessage("Mã OTP phải là 4 chữ số");
    }
}

