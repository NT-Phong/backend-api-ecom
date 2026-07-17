using FluentValidation;

namespace Ecom.Application.Features.Auth.Commands.UpdateUserRole;

public class UpdateUserRoleCommandValidator : AbstractValidator<UpdateUserRoleCommand>
{
    public UpdateUserRoleCommandValidator()
    {
        RuleFor(x => x.TargetUserId)
            .NotEmpty().WithMessage("ID người dùng cần đổi quyền là bắt buộc.");

        RuleFor(x => x.NewRoleId)
            .NotEmpty().WithMessage("ID quyền hạn mới là bắt buộc.");
    }
}
