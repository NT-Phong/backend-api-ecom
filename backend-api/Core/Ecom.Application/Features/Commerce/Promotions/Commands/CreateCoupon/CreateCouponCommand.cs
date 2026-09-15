using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Promotions.Commands.CreateCoupon;

public sealed record CreateCouponCommand(
    string Code,
    Guid? PromotionId,
    int? UsageLimit,
    int? PerUserLimit,
    DateTime StartsAt,
    DateTime? EndsAt,
    CouponStatus Status = CouponStatus.Active,
    IReadOnlyList<Guid>? AppliedProductIds = null,
    IReadOnlyList<Guid>? AppliedCategoryIds = null) : IRequest<TResult<Guid>>, ITransactionalRequest;

public sealed class CreateCouponCommandValidator : AbstractValidator<CreateCouponCommand>
{
    public CreateCouponCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().WithMessage("Mã coupon không được để trống.").MaximumLength(50);
        RuleFor(x => x.UsageLimit).GreaterThan(0).When(x => x.UsageLimit.HasValue);
        RuleFor(x => x.PerUserLimit).GreaterThan(0).When(x => x.PerUserLimit.HasValue);
        RuleFor(x => x.StartsAt).NotEmpty().WithMessage("Ngày bắt đầu không được để trống.");
        RuleFor(x => x)
            .Must(x => !x.EndsAt.HasValue || x.EndsAt > x.StartsAt)
            .WithMessage("Ngày kết thúc phải sau ngày bắt đầu.");
        RuleFor(x => x.Status).IsInEnum();
    }
}

public sealed class CreateCouponCommandHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    : IRequestHandler<CreateCouponCommand, TResult<Guid>>
{
    public async Task<TResult<Guid>> Handle(CreateCouponCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated)
            return TResult<Guid>.Failure(MessageKey.Unauthorized, ErrorCodes.UNAUTHORIZED);
        if (!currentUser.HasPolicy(Permissions.Promotions.Manage))
            return TResult<Guid>.Failure(MessageKey.Forbidden, ErrorCodes.FORBIDDEN);

        if (request.PromotionId.HasValue)
        {
            var promotion = await unitOfWork.Repository<Promotion>().FindByIdAsync(request.PromotionId.Value);
            if (promotion is null)
                return TResult<Guid>.Failure("Chương trình khuyến mãi không tồn tại.", ErrorCodes.NOT_FOUND);
        }

        var normalizedCode = request.Code.Trim().ToUpperInvariant();
        var codeExists = await unitOfWork.Repository<Coupon>().QueryNoTracking()
            .AnyAsync(x => x.Code == normalizedCode, cancellationToken);
        if (codeExists)
            return TResult<Guid>.Failure("Mã giảm giá này đã tồn tại.", ErrorCodes.ALREADY_EXISTS);

        var coupon = Coupon.Create(
            request.Code,
            request.PromotionId,
            request.UsageLimit,
            request.PerUserLimit,
            request.StartsAt,
            request.EndsAt,
            request.Status);

        await unitOfWork.Repository<Coupon>().InsertAsync(coupon, cancellationToken);

        if (request.AppliedProductIds is { Count: > 0 })
        {
            var productLinks = request.AppliedProductIds.Distinct()
                .Select(pid => CouponProduct.Create(coupon.Id, pid))
                .ToList();
            await unitOfWork.Repository<CouponProduct>().InsertRangeAsync(productLinks, cancellationToken);
        }

        if (request.AppliedCategoryIds is { Count: > 0 })
        {
            var categoryLinks = request.AppliedCategoryIds.Distinct()
                .Select(cid => CouponCategory.Create(coupon.Id, cid))
                .ToList();
            await unitOfWork.Repository<CouponCategory>().InsertRangeAsync(categoryLinks, cancellationToken);
        }

        return TResult<Guid>.Success(coupon.Id);
    }
}
