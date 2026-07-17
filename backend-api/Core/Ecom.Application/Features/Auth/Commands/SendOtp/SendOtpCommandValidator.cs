namespace Ecom.Application.Features.Auth.Commands.SendOtp;

public class SendOtpCommandValidator : AbstractValidator<SendOtpCommand>
{
    public SendOtpCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage(MessageKey.PhoneNumberRequired)

            .Matches(@"^(03|05|07|08|09)\d{8}$")
            .WithMessage(MessageKey.PhoneNumberInvalid);
    }
}

