namespace Ecom.Application.Features.Auth.Commands.SendOtp;

public class SendOtpCommandValidator : AbstractValidator<SendOtpCommand>
{
    public SendOtpCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage(MessageKey.PhoneNumberRequired)

            .Must(value => Ecom.Domain.Extensions.VietnamesePhoneNumber.TryNormalize(value, out _))
            .WithMessage(MessageKey.PhoneNumberInvalid);
    }
}

