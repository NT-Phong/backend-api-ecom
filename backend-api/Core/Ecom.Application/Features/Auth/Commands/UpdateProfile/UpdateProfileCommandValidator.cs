namespace Ecom.Application.Features.Auth.Commands.UpdateProfile;

public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    private const string EmailRegex = @"^[a-zA-Z0-9]+([._-][a-zA-Z0-9]+)*@[a-zA-Z0-9]+([.-][a-zA-Z0-9]+)*\.[a-zA-Z]{2,10}$";
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.FullName)
            .MaximumLength(200).WithMessage("Họ và tên không được vượt quá 200 ký tự")
            .When(x => x.FullName != null);

        RuleFor(x => x.Email)
            .Matches(EmailRegex).WithMessage("Định dạng email không hợp lệ (Ví dụ: user@example.com).")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Address)
            .MaximumLength(500).WithMessage("Địa chỉ không được vượt quá 500 ký tự")
            .When(x => x.Address != null);
    }
}
