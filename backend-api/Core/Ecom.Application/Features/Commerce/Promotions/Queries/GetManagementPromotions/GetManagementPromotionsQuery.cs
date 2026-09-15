using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Promotions.Queries.GetManagementPromotions;

public sealed record GetManagementPromotionsQuery : IRequest<TResult<PaginatedList<ManagementPromotionDto>>>
{
    public string? Q { get; init; }
    public PromotionStatus? Status { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public int Skip() => (Page - 1) * PageSize;
}

public sealed class GetManagementPromotionsQueryValidator : AbstractValidator<GetManagementPromotionsQuery>
{
    public GetManagementPromotionsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

public sealed class GetManagementPromotionsQueryHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    : IRequestHandler<GetManagementPromotionsQuery, TResult<PaginatedList<ManagementPromotionDto>>>
{
    public async Task<TResult<PaginatedList<ManagementPromotionDto>>> Handle(GetManagementPromotionsQuery request, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated)
            return TResult<PaginatedList<ManagementPromotionDto>>.Failure(MessageKey.Unauthorized, ErrorCodes.UNAUTHORIZED);
        if (!currentUser.HasPolicy(Permissions.Promotions.Read))
            return TResult<PaginatedList<ManagementPromotionDto>>.Failure(MessageKey.Forbidden, ErrorCodes.FORBIDDEN);

        var query = unitOfWork.Repository<Promotion>().QueryNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Q))
        {
            var q = request.Q.Trim().ToUpperInvariant();
            query = query.Where(x => x.Code.Contains(q) || x.Name.Contains(q));
        }

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip(request.Skip())
            .Take(request.PageSize)
            .Select(x => new ManagementPromotionDto(
                x.Id,
                x.Code,
                x.Name,
                x.PromotionType,
                x.Value,
                x.MinOrderAmount,
                x.Status,
                x.StartsAt,
                x.EndsAt,
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(ct);

        return TResult<PaginatedList<ManagementPromotionDto>>.Success(
            PaginatedList<ManagementPromotionDto>.Create(items, total, request.Page, request.PageSize));
    }
}
