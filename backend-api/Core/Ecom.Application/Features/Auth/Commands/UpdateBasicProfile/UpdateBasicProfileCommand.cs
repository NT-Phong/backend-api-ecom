namespace Ecom.Application.Features.Auth.Commands.UpdateBasicProfile;

public sealed record UpdateBasicProfileCommand(string FullName) : IRequest<TResult<UpdateBasicProfileResult>>;

public sealed class UpdateBasicProfileCommandValidator : AbstractValidator<UpdateBasicProfileCommand>
{
    public UpdateBasicProfileCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Họ và tên không được để trống.")
            .MaximumLength(200).WithMessage("Họ và tên không được vượt quá 200 ký tự.");
    }
}

public sealed record UpdateBasicProfileResult(Guid UserId, string FullName, string ProfileState);
