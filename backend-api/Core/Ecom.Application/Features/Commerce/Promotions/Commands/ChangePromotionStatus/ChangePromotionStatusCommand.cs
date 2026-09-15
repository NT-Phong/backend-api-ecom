using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Promotions.Commands.ChangePromotionStatus;

public sealed record ChangePromotionStatusCommand(Guid Id, PromotionStatus Status)
    : IRequest<TResult<bool>>, ITransactionalRequest;

public sealed class ChangePromotionStatusCommandValidator : AbstractValidator<ChangePromotionStatusCommand>
{
    public ChangePromotionStatusCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Status).IsInEnum();
    }
}

public sealed class ChangePromotionStatusCommandHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    : IRequestHandler<ChangePromotionStatusCommand, TResult<bool>>
{
    public async Task<TResult<bool>> Handle(ChangePromotionStatusCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated)
            return TResult<bool>.Failure(MessageKey.Unauthorized, ErrorCodes.UNAUTHORIZED);
        if (!currentUser.HasPolicy(Permissions.Promotions.Manage))
            return TResult<bool>.Failure(MessageKey.Forbidden, ErrorCodes.FORBIDDEN);

        var promotion = await unitOfWork.Repository<Promotion>().FindByIdAsync(request.Id);
        if (promotion is null)
            return TResult<bool>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);

        switch (request.Status)
        {
            case PromotionStatus.Active:
                promotion.Activate();
                break;
            case PromotionStatus.Paused:
                promotion.Pause();
                break;
            case PromotionStatus.Expired:
                promotion.Expire();
                break;
            default:
                return TResult<bool>.Failure("Trạng thái không hợp lệ.", ErrorCodes.BAD_REQUEST);
        }

        await unitOfWork.Repository<Promotion>().UpdateAsync(promotion, cancellationToken);
        return TResult<bool>.Success(true);
    }
}
