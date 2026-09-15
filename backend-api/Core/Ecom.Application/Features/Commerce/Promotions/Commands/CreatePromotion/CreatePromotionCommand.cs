using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Promotions.Commands.CreatePromotion;

public sealed record CreatePromotionCommand(
    string Code,
    string Name,
    PromotionType PromotionType,
    decimal Value,
    DateTime StartsAt,
    DateTime? EndsAt,
    decimal? MinOrderAmount,
    PromotionStatus Status = PromotionStatus.Active) : IRequest<TResult<Guid>>, ITransactionalRequest;

public sealed class CreatePromotionCommandValidator : AbstractValidator<CreatePromotionCommand>
{
    public CreatePromotionCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().WithMessage("Mã khuyến mãi không được để trống.").MaximumLength(50);
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
        RuleFor(x => x.Status).IsInEnum();
    }
}

public sealed class CreatePromotionCommandHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    : IRequestHandler<CreatePromotionCommand, TResult<Guid>>
{
    public async Task<TResult<Guid>> Handle(CreatePromotionCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated)
            return TResult<Guid>.Failure(MessageKey.Unauthorized, ErrorCodes.UNAUTHORIZED);
        if (!currentUser.HasPolicy(Permissions.Promotions.Manage))
            return TResult<Guid>.Failure(MessageKey.Forbidden, ErrorCodes.FORBIDDEN);

        var normalizedCode = request.Code.Trim().ToUpperInvariant();
        var codeExists = await unitOfWork.Repository<Promotion>().QueryNoTracking()
            .AnyAsync(x => x.Code == normalizedCode, cancellationToken);
        if (codeExists)
            return TResult<Guid>.Failure("Mã khuyến mãi đã tồn tại.", ErrorCodes.ALREADY_EXISTS);

        var promotion = Promotion.Create(
            request.Code,
            request.Name,
            request.PromotionType,
            request.Value,
            request.StartsAt,
            request.EndsAt,
            request.MinOrderAmount,
            request.Status);

        await unitOfWork.Repository<Promotion>().InsertAsync(promotion, cancellationToken);
        return TResult<Guid>.Success(promotion.Id);
    }
}
