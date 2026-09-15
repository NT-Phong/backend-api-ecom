using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Promotions.Commands.UpdateCoupon;

public sealed record UpdateCouponCommand(
    Guid Id,
    Guid? PromotionId,
    int? UsageLimit,
    int? PerUserLimit,
    DateTime StartsAt,
    DateTime? EndsAt,
    IReadOnlyList<Guid>? AppliedProductIds = null,
    IReadOnlyList<Guid>? AppliedCategoryIds = null) : IRequest<TResult<bool>>, ITransactionalRequest;

public sealed class UpdateCouponCommandValidator : AbstractValidator<UpdateCouponCommand>
{
    public UpdateCouponCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.UsageLimit).GreaterThan(0).When(x => x.UsageLimit.HasValue);
        RuleFor(x => x.PerUserLimit).GreaterThan(0).When(x => x.PerUserLimit.HasValue);
        RuleFor(x => x.StartsAt).NotEmpty().WithMessage("Ngày bắt đầu không được để trống.");
        RuleFor(x => x)
            .Must(x => !x.EndsAt.HasValue || x.EndsAt > x.StartsAt)
            .WithMessage("Ngày kết thúc phải sau ngày bắt đầu.");
    }
}

public sealed class UpdateCouponCommandHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    : IRequestHandler<UpdateCouponCommand, TResult<bool>>
{
    public async Task<TResult<bool>> Handle(UpdateCouponCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated)
            return TResult<bool>.Failure(MessageKey.Unauthorized, ErrorCodes.UNAUTHORIZED);
        if (!currentUser.HasPolicy(Permissions.Promotions.Manage))
            return TResult<bool>.Failure(MessageKey.Forbidden, ErrorCodes.FORBIDDEN);

        var coupon = await unitOfWork.Repository<Coupon>().FindByIdAsync(request.Id);
        if (coupon is null)
            return TResult<bool>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);

        if (request.PromotionId.HasValue)
        {
            var promotion = await unitOfWork.Repository<Promotion>().FindByIdAsync(request.PromotionId.Value);
            if (promotion is null)
                return TResult<bool>.Failure("Chương trình khuyến mãi không tồn tại.", ErrorCodes.NOT_FOUND);
        }

        coupon.Update(
            request.PromotionId,
            request.UsageLimit,
            request.PerUserLimit,
            request.StartsAt,
            request.EndsAt);

        await unitOfWork.Repository<Coupon>().UpdateAsync(coupon, cancellationToken);

        // Sync Product links
        var existingProducts = await unitOfWork.Repository<CouponProduct>().QueryNoTracking()
            .Where(x => x.CouponId == coupon.Id)
            .ToListAsync(cancellationToken);
        if (existingProducts.Count > 0)
        {
            await unitOfWork.Repository<CouponProduct>().DeleteRangeAsync(existingProducts, cancellationToken);
        }

        if (request.AppliedProductIds is { Count: > 0 })
        {
            var newProducts = request.AppliedProductIds.Distinct()
                .Select(pid => CouponProduct.Create(coupon.Id, pid))
                .ToList();
            await unitOfWork.Repository<CouponProduct>().InsertRangeAsync(newProducts, cancellationToken);
        }

        // Sync Category links
        var existingCategories = await unitOfWork.Repository<CouponCategory>().QueryNoTracking()
            .Where(x => x.CouponId == coupon.Id)
            .ToListAsync(cancellationToken);
        if (existingCategories.Count > 0)
        {
            await unitOfWork.Repository<CouponCategory>().DeleteRangeAsync(existingCategories, cancellationToken);
        }

        if (request.AppliedCategoryIds is { Count: > 0 })
        {
            var newCategories = request.AppliedCategoryIds.Distinct()
                .Select(cid => CouponCategory.Create(coupon.Id, cid))
                .ToList();
            await unitOfWork.Repository<CouponCategory>().InsertRangeAsync(newCategories, cancellationToken);
        }

        return TResult<bool>.Success(true);
    }
}
