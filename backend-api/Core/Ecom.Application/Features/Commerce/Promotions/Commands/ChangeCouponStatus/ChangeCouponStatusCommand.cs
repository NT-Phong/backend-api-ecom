using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Promotions.Commands.ChangeCouponStatus;

public sealed record ChangeCouponStatusCommand(Guid Id, CouponStatus Status)
    : IRequest<TResult<bool>>, ITransactionalRequest;

public sealed class ChangeCouponStatusCommandValidator : AbstractValidator<ChangeCouponStatusCommand>
{
    public ChangeCouponStatusCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Status).IsInEnum();
    }
}

public sealed class ChangeCouponStatusCommandHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    : IRequestHandler<ChangeCouponStatusCommand, TResult<bool>>
{
    public async Task<TResult<bool>> Handle(ChangeCouponStatusCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated)
            return TResult<bool>.Failure(MessageKey.Unauthorized, ErrorCodes.UNAUTHORIZED);
        if (!currentUser.HasPolicy(Permissions.Promotions.Manage))
            return TResult<bool>.Failure(MessageKey.Forbidden, ErrorCodes.FORBIDDEN);

        var coupon = await unitOfWork.Repository<Coupon>().FindByIdAsync(request.Id);
        if (coupon is null)
            return TResult<bool>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);

        switch (request.Status)
        {
            case CouponStatus.Active:
                coupon.Activate();
                break;
            case CouponStatus.Paused:
                coupon.Pause();
                break;
            case CouponStatus.Expired:
                coupon.Expire();
                break;
            default:
                return TResult<bool>.Failure("Trạng thái không hợp lệ.", ErrorCodes.BAD_REQUEST);
        }

        await unitOfWork.Repository<Coupon>().UpdateAsync(coupon, cancellationToken);
        return TResult<bool>.Success(true);
    }
}
