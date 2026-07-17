namespace Ecom.Application.Features.Auth.Commands.UserManagement.Commands.UpdateUser;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(v => v.FullName).NotEmpty().WithMessage("Họ và tên không được để trống");
        RuleFor(v => v.RoleId).NotEmpty().WithMessage("Vui lòng chọn vai trò");

    }
}
