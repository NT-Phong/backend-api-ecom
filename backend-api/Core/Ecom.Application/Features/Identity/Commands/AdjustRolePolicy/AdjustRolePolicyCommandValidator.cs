
namespace Ecom.Application.Features.Identity.Commands.AdjustRolePolicy;

public class AdjustRolePolicyCommandValidator : AbstractValidator<AdjustRolePolicyCommand>
{
    public AdjustRolePolicyCommandValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("RoleId không được trống.");

        RuleFor(x => x.Policies)
            .NotNull().WithMessage("Danh sách quyền không được null.")
            .NotEmpty().WithMessage("Danh sách quyền phải có ít nhất một giá trị.");
    }
}

