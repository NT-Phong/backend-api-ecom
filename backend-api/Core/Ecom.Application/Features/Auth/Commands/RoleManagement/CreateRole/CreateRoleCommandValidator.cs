namespace Ecom.Application.Features.Auth.Commands.RoleManagement.CreateRole;

public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(v => v.Name).NotEmpty().WithMessage("Tên phân quyền không được để trống");
        RuleFor(v => v.Priority).GreaterThan(0).WithMessage("Thứ tự ưu tiên phải lớn hơn 0");
    }
}
