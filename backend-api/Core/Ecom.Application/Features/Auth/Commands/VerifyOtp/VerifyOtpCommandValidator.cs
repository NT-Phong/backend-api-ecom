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

            .Matches(@"^(03|05|07|08|09)\d{8}$")
            .WithMessage(MessageKey.PhoneNumberInvalid);

        RuleFor(x => x.OtpCode)
            .NotEmpty().WithMessage("Mã OTP không được để trống")
            .Matches(@"^\d{4}$").WithMessage("Mã OTP phải là 4 chữ số");
    }
}

