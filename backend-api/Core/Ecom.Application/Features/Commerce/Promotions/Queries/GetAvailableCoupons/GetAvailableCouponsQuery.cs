using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Promotions.Queries.GetAvailableCoupons;

public sealed record GetAvailableCouponsQuery : IRequest<TResult<IReadOnlyList<AvailableCouponDto>>>;

public sealed class GetAvailableCouponsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAvailableCouponsQuery, TResult<IReadOnlyList<AvailableCouponDto>>>
{
    public async Task<TResult<IReadOnlyList<AvailableCouponDto>>> Handle(GetAvailableCouponsQuery request, CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        var coupons = await (
            from c in unitOfWork.Repository<Coupon>().QueryNoTracking()
            where c.Status == CouponStatus.Active && c.StartsAt <= now && (c.EndsAt == null || c.EndsAt >= now)
            join p in unitOfWork.Repository<Promotion>().QueryNoTracking() on c.PromotionId equals p.Id into pGroup
            from p in pGroup.DefaultIfEmpty()
            where p == null || (p.Status == PromotionStatus.Active && p.StartsAt <= now && (p.EndsAt == null || p.EndsAt >= now))
            orderby c.StartsAt descending
            select new
            {
                c.Id,
                c.Code,
                PromotionName = p != null ? p.Name : null,
                PromotionType = p != null ? (PromotionType?)p.PromotionType : null,
                PromotionValue = p != null ? (decimal?)p.Value : null,
                MinOrderAmount = p != null ? p.MinOrderAmount : null,
                c.UsageLimit,
                c.StartsAt,
                c.EndsAt
            })
            .ToListAsync(ct);

        var couponIds = coupons.Select(x => x.Id).ToList();

        var redemptionCounts = await unitOfWork.Repository<CouponRedemption>().QueryNoTracking()
            .Where(x => couponIds.Contains(x.CouponId))
            .GroupBy(x => x.CouponId)
            .Select(g => new { CouponId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CouponId, x => x.Count, ct);

        var available = coupons
            .Where(c => !c.UsageLimit.HasValue || redemptionCounts.GetValueOrDefault(c.Id, 0) < c.UsageLimit.Value)
            .Select(c => new AvailableCouponDto(
                c.Id,
                c.Code,
                c.PromotionName,
                c.PromotionType,
                c.PromotionValue,
                c.MinOrderAmount,
                c.StartsAt,
                c.EndsAt))
            .ToList();

        return TResult<IReadOnlyList<AvailableCouponDto>>.Success(available);
    }
}
