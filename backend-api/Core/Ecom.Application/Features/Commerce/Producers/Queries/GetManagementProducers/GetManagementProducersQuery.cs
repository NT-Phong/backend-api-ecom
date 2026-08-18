using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Producers.Queries.GetManagementProducers;

public sealed class GetManagementProducersQuery : IRequest<TResult<PaginatedList<ProducerListItemDto>>>
{
    public string? Q { get; init; }
    public PublicStatus? PublicStatus { get; init; }
    public bool? IsVerified { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public int Skip() => (Page - 1) * PageSize;
}

public sealed class GetManagementProducersQueryValidator : AbstractValidator<GetManagementProducersQuery>
{
    public GetManagementProducersQueryValidator()
    {
        RuleFor(x => x.Q).MaximumLength(250);
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

public sealed class GetManagementProducersQueryHandler(IUnitOfWork unitOfWork, ProducerManagementService service)
    : IRequestHandler<GetManagementProducersQuery, TResult<PaginatedList<ProducerListItemDto>>>
{
    public async Task<TResult<PaginatedList<ProducerListItemDto>>> Handle(GetManagementProducersQuery request, CancellationToken ct)
    {
        var authorization = service.Ensure(Permissions.Producers.Read);
        if (!authorization.IsSuccess) return TResult<PaginatedList<ProducerListItemDto>>.Failure(authorization.Error!, authorization.ErrorCode);
        var query = unitOfWork.Repository<Producer>().QueryNoTracking();
        if (!string.IsNullOrWhiteSpace(request.Q))
        {
            var q = request.Q.Trim().ToLower();
            query = query.Where(x => x.Code.ToLower().Contains(q) || x.Name.ToLower().Contains(q));
        }
        if (request.PublicStatus.HasValue) query = query.Where(x => x.PublicStatus == request.PublicStatus.Value);
        if (request.IsVerified.HasValue) query = query.Where(x => x.IsVerified == request.IsVerified.Value);
        var total = await query.CountAsync(ct);
        var producers = await query.OrderBy(x => x.Name).ThenBy(x => x.Id).Skip(request.Skip()).Take(request.PageSize).ToListAsync(ct);
        var ids = producers.Select(x => x.Id).ToArray();
        var facilityCounts = await unitOfWork.Repository<ProductionFacility>().QueryNoTracking().Where(x => ids.Contains(x.ProducerId))
            .GroupBy(x => x.ProducerId).Select(x => new { x.Key, Count = x.Count() }).ToDictionaryAsync(x => x.Key, x => x.Count, ct);
        var productCounts = await unitOfWork.Repository<Product>().QueryNoTracking().Where(x => ids.Contains(x.ProducerId))
            .GroupBy(x => x.ProducerId).Select(x => new { x.Key, Count = x.Count() }).ToDictionaryAsync(x => x.Key, x => x.Count, ct);
        var items = producers.Select(x => new ProducerListItemDto(x.Id, x.Code, x.Name, x.LegalName, x.PublicStatus,
            x.IsVerified, x.VerifiedAt, facilityCounts.GetValueOrDefault(x.Id), productCounts.GetValueOrDefault(x.Id),
            x.ConcurrencyStamp, x.CreatedAt, x.UpdatedAt)).ToList();
        return TResult<PaginatedList<ProducerListItemDto>>.Success(PaginatedList<ProducerListItemDto>.Create(items, total, request.Page, request.PageSize));
    }
}
