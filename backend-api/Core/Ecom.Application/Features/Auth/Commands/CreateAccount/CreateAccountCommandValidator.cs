namespace Ecom.Application.Features.Auth.Commands.CreateAccount;

public class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountCommandValidator()
    {
        // Phone number validation - required
        RuleLevelCascadeMode = CascadeMode.Stop;
        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage(MessageKey.PhoneNumberRequired)

            .Must(value => Ecom.Domain.Extensions.VietnamesePhoneNumber.TryNormalize(value, out _))
            .WithMessage(MessageKey.PhoneNumberInvalid);
    }
}

