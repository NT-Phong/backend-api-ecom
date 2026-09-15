using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Promotions.Queries.GetManagementCouponById;

public sealed record GetManagementCouponByIdQuery(Guid Id) : IRequest<TResult<ManagementCouponDto>>;

public sealed class GetManagementCouponByIdQueryHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    : IRequestHandler<GetManagementCouponByIdQuery, TResult<ManagementCouponDto>>
{
    public async Task<TResult<ManagementCouponDto>> Handle(GetManagementCouponByIdQuery request, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated)
            return TResult<ManagementCouponDto>.Failure(MessageKey.Unauthorized, ErrorCodes.UNAUTHORIZED);
        if (!currentUser.HasPolicy(Permissions.Promotions.Read))
            return TResult<ManagementCouponDto>.Failure(MessageKey.Forbidden, ErrorCodes.FORBIDDEN);

        var coupon = await unitOfWork.Repository<Coupon>().FindByIdAsync(request.Id);
        if (coupon is null)
            return TResult<ManagementCouponDto>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);

        string? promotionName = null;
        if (coupon.PromotionId.HasValue)
        {
            var promotion = await unitOfWork.Repository<Promotion>().FindByIdAsync(coupon.PromotionId.Value);
            promotionName = promotion?.Name;
        }

        var usageCount = await unitOfWork.Repository<CouponRedemption>().QueryNoTracking()
            .CountAsync(x => x.CouponId == coupon.Id, ct);

        var productIds = await unitOfWork.Repository<CouponProduct>().QueryNoTracking()
            .Where(x => x.CouponId == coupon.Id)
            .Select(x => x.ProductId)
            .ToListAsync(ct);

        var categoryIds = await unitOfWork.Repository<CouponCategory>().QueryNoTracking()
            .Where(x => x.CouponId == coupon.Id)
            .Select(x => x.CategoryId)
            .ToListAsync(ct);

        return TResult<ManagementCouponDto>.Success(new ManagementCouponDto(
            coupon.Id,
            coupon.PromotionId,
            promotionName,
            coupon.Code,
            coupon.UsageLimit,
            usageCount,
            coupon.PerUserLimit,
            coupon.Status,
            coupon.StartsAt,
            coupon.EndsAt,
            productIds,
            categoryIds,
            coupon.CreatedAt,
            coupon.UpdatedAt));
    }
}
