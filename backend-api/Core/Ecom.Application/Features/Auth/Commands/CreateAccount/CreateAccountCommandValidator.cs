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

            .Matches(@"^(03|05|07|08|09)\d{8}$")
            .WithMessage(MessageKey.PhoneNumberInvalid);
    }
}

