using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Auth.Commands.UpdateBasicProfile;

[EnableUnitOfWork]
public sealed class UpdateBasicProfileCommandHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    : IRequestHandler<UpdateBasicProfileCommand, TResult<UpdateBasicProfileResult>>
{
    public async Task<TResult<UpdateBasicProfileResult>> Handle(UpdateBasicProfileCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId == Guid.Empty)
            return TResult<UpdateBasicProfileResult>.Failure(MessageKey.Unauthorized, ErrorCodes.UNAUTHORIZED);

        var user = await unitOfWork.Repository<User>().FindByIdAsync(currentUser.UserId);
        if (user is null)
            return TResult<UpdateBasicProfileResult>.Failure(MessageKey.UserNotFound, ErrorCodes.NOT_FOUND);

        user.SetBasicProfile(request.FullName);
        user.UpdatedAt = DateTime.UtcNow;
        await unitOfWork.Repository<User>().UpdateAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return TResult<UpdateBasicProfileResult>.Success(new(user.Id, user.FullName!, "READY"));
    }
}
