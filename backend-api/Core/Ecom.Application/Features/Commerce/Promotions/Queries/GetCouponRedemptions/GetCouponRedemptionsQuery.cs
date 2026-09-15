using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Promotions.Queries.GetCouponRedemptions;

public sealed record GetCouponRedemptionsQuery : IRequest<TResult<PaginatedList<CouponRedemptionDto>>>
{
    public Guid? CouponId { get; init; }
    public Guid? UserId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public int Skip() => (Page - 1) * PageSize;
}

public sealed class GetCouponRedemptionsQueryValidator : AbstractValidator<GetCouponRedemptionsQuery>
{
    public GetCouponRedemptionsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

public sealed class GetCouponRedemptionsQueryHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    : IRequestHandler<GetCouponRedemptionsQuery, TResult<PaginatedList<CouponRedemptionDto>>>
{
    public async Task<TResult<PaginatedList<CouponRedemptionDto>>> Handle(GetCouponRedemptionsQuery request, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated)
            return TResult<PaginatedList<CouponRedemptionDto>>.Failure(MessageKey.Unauthorized, ErrorCodes.UNAUTHORIZED);
        if (!currentUser.HasPolicy(Permissions.Promotions.Read))
            return TResult<PaginatedList<CouponRedemptionDto>>.Failure(MessageKey.Forbidden, ErrorCodes.FORBIDDEN);

        var query = unitOfWork.Repository<CouponRedemption>().QueryNoTracking();

        if (request.CouponId.HasValue)
        {
            query = query.Where(x => x.CouponId == request.CouponId.Value);
        }

        if (request.UserId.HasValue)
        {
            query = query.Where(x => x.UserId == request.UserId.Value);
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(x => x.RedeemedAt)
            .Skip(request.Skip())
            .Take(request.PageSize)
            .Select(x => new CouponRedemptionDto(
                x.Id,
                x.CouponId,
                x.UserId,
                x.OrderId,
                x.DiscountAmount,
                x.RedeemedAt))
            .ToListAsync(ct);

        return TResult<PaginatedList<CouponRedemptionDto>>.Success(
            PaginatedList<CouponRedemptionDto>.Create(items, total, request.Page, request.PageSize));
    }
}
