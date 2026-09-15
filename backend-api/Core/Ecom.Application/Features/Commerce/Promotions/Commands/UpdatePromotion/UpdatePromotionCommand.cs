using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Promotions.Commands.UpdatePromotion;

public sealed record UpdatePromotionCommand(
    Guid Id,
    string Name,
    PromotionType PromotionType,
    decimal Value,
    DateTime StartsAt,
    DateTime? EndsAt,
    decimal? MinOrderAmount) : IRequest<TResult<bool>>, ITransactionalRequest;

public sealed class UpdatePromotionCommandValidator : AbstractValidator<UpdatePromotionCommand>
{
    public UpdatePromotionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().WithMessage("Tên chương trình khuyến mãi không được để trống.").MaximumLength(200);
        RuleFor(x => x.PromotionType).IsInEnum();
        RuleFor(x => x.Value).GreaterThan(0).WithMessage("Giá trị khuyến mãi phải lớn hơn 0.");
        RuleFor(x => x.Value)
            .LessThanOrEqualTo(100)
            .When(x => x.PromotionType == PromotionType.Percentage)
            .WithMessage("Phần trăm khuyến mãi không thể vượt quá 100%.");
        RuleFor(x => x.MinOrderAmount).GreaterThanOrEqualTo(0).When(x => x.MinOrderAmount.HasValue);
        RuleFor(x => x.StartsAt).NotEmpty().WithMessage("Ngày bắt đầu không được để trống.");
        RuleFor(x => x)
            .Must(x => !x.EndsAt.HasValue || x.EndsAt > x.StartsAt)
            .WithMessage("Ngày kết thúc phải sau ngày bắt đầu.");
    }
}

public sealed class UpdatePromotionCommandHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    : IRequestHandler<UpdatePromotionCommand, TResult<bool>>
{
    public async Task<TResult<bool>> Handle(UpdatePromotionCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated)
            return TResult<bool>.Failure(MessageKey.Unauthorized, ErrorCodes.UNAUTHORIZED);
        if (!currentUser.HasPolicy(Permissions.Promotions.Manage))
            return TResult<bool>.Failure(MessageKey.Forbidden, ErrorCodes.FORBIDDEN);

        var promotion = await unitOfWork.Repository<Promotion>().FindByIdAsync(request.Id);
        if (promotion is null)
            return TResult<bool>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);

        promotion.Update(
            request.Name,
            request.PromotionType,
            request.Value,
            request.StartsAt,
            request.EndsAt,
            request.MinOrderAmount);

        await unitOfWork.Repository<Promotion>().UpdateAsync(promotion, cancellationToken);
        return TResult<bool>.Success(true);
    }
}
