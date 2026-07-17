namespace Ecom.Application.Features.Auth.Commands.UserManagement.Commands.CreateUser;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(v => v.FullName)
            .NotEmpty().WithMessage("Họ và tên không được để trống")
            .MaximumLength(100).WithMessage("Họ và tên không quá 100 ký tự");

        RuleFor(v => v.PhoneNumber)
            .NotEmpty().WithMessage("Số điện thoại không được để trống")
            .Matches(@"^(03|05|07|08|09)\d{8}$")
            .WithMessage(MessageKey.PhoneNumberInvalid);

        RuleFor(v => v.RoleId)
            .NotEmpty().WithMessage("Vui lòng chọn vai trò cho người dùng");

    }
}
