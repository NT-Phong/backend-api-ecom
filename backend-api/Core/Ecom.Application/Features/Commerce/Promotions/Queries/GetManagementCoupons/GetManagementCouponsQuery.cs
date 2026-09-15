using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Promotions.Queries.GetManagementCoupons;

public sealed record GetManagementCouponsQuery : IRequest<TResult<PaginatedList<ManagementCouponDto>>>
{
    public Guid? PromotionId { get; init; }
    public string? Q { get; init; }
    public CouponStatus? Status { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public int Skip() => (Page - 1) * PageSize;
}

public sealed class GetManagementCouponsQueryValidator : AbstractValidator<GetManagementCouponsQuery>
{
    public GetManagementCouponsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

public sealed class GetManagementCouponsQueryHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    : IRequestHandler<GetManagementCouponsQuery, TResult<PaginatedList<ManagementCouponDto>>>
{
    public async Task<TResult<PaginatedList<ManagementCouponDto>>> Handle(GetManagementCouponsQuery request, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated)
            return TResult<PaginatedList<ManagementCouponDto>>.Failure(MessageKey.Unauthorized, ErrorCodes.UNAUTHORIZED);
        if (!currentUser.HasPolicy(Permissions.Promotions.Read))
            return TResult<PaginatedList<ManagementCouponDto>>.Failure(MessageKey.Forbidden, ErrorCodes.FORBIDDEN);

        var query = unitOfWork.Repository<Coupon>().QueryNoTracking();

        if (request.PromotionId.HasValue)
        {
            query = query.Where(x => x.PromotionId == request.PromotionId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Q))
        {
            var q = request.Q.Trim().ToUpperInvariant();
            query = query.Where(x => x.Code.Contains(q));
        }

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }

        var total = await query.CountAsync(ct);

        var rawCoupons = await (
            from c in query
            join p in unitOfWork.Repository<Promotion>().QueryNoTracking() on c.PromotionId equals p.Id into pGroup
            from p in pGroup.DefaultIfEmpty()
            orderby c.CreatedAt descending
            select new
            {
                c.Id,
                c.PromotionId,
                PromotionName = p != null ? p.Name : null,
                c.Code,
                c.UsageLimit,
                c.PerUserLimit,
                c.Status,
                c.StartsAt,
                c.EndsAt,
                c.CreatedAt,
                c.UpdatedAt
            })
            .Skip(request.Skip())
            .Take(request.PageSize)
            .ToListAsync(ct);

        var couponIds = rawCoupons.Select(x => x.Id).ToList();

        var redemptionCounts = await unitOfWork.Repository<CouponRedemption>().QueryNoTracking()
            .Where(x => couponIds.Contains(x.CouponId))
            .GroupBy(x => x.CouponId)
            .Select(g => new { CouponId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CouponId, x => x.Count, ct);

        var couponProducts = await unitOfWork.Repository<CouponProduct>().QueryNoTracking()
            .Where(x => couponIds.Contains(x.CouponId))
            .ToListAsync(ct);

        var couponCategories = await unitOfWork.Repository<CouponCategory>().QueryNoTracking()
            .Where(x => couponIds.Contains(x.CouponId))
            .ToListAsync(ct);

        var items = rawCoupons.Select(c => new ManagementCouponDto(
            c.Id,
            c.PromotionId,
            c.PromotionName,
            c.Code,
            c.UsageLimit,
            redemptionCounts.GetValueOrDefault(c.Id, 0),
            c.PerUserLimit,
            c.Status,
            c.StartsAt,
            c.EndsAt,
            couponProducts.Where(p => p.CouponId == c.Id).Select(p => p.ProductId).ToList(),
            couponCategories.Where(cat => cat.CouponId == c.Id).Select(cat => cat.CategoryId).ToList(),
            c.CreatedAt,
            c.UpdatedAt
        )).ToList();

        return TResult<PaginatedList<ManagementCouponDto>>.Success(
            PaginatedList<ManagementCouponDto>.Create(items, total, request.Page, request.PageSize));
    }
}
