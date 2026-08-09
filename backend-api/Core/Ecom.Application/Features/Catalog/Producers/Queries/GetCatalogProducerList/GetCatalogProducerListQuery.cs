using Ecom.Application.Features.Catalog.Common;
using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Catalog.Producers.Queries.GetCatalogProducerList;

public sealed class GetCatalogProducerListQuery : IRequest<TResult<PaginatedList<CatalogProducerPickerDto>>>
{
    public string? Q { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public int Skip() => (Page - 1) * PageSize;
}

public sealed class GetCatalogProducerListQueryValidator : AbstractValidator<GetCatalogProducerListQuery>
{
    public GetCatalogProducerListQueryValidator()
    {
        RuleFor(x => x.Q).MaximumLength(250);
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

public sealed class GetCatalogProducerListQueryHandler(
    IUnitOfWork unitOfWork,
    ICatalogProductAccessService access)
    : IRequestHandler<GetCatalogProducerListQuery, TResult<PaginatedList<CatalogProducerPickerDto>>>
{
    public async Task<TResult<PaginatedList<CatalogProducerPickerDto>>> Handle(
        GetCatalogProducerListQuery request,
        CancellationToken cancellationToken)
    {
        var authorization = access.Ensure(Permissions.CatalogProducts.Create);
        if (!authorization.IsSuccess)
            return TResult<PaginatedList<CatalogProducerPickerDto>>.Failure(authorization.Error!, authorization.ErrorCode);

        var query = unitOfWork.Repository<Producer>().QueryNoTracking()
            .Where(x => x.PublicStatus == PublicStatus.Published && x.IsVerified);
        if (!string.IsNullOrWhiteSpace(request.Q))
        {
            var search = request.Q.Trim().ToLower();
            query = query.Where(x => x.Code.ToLower().Contains(search) || x.Name.ToLower().Contains(search));
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(x => x.Name).ThenBy(x => x.Id)
            .Skip(request.Skip()).Take(request.PageSize)
            .Select(x => new CatalogProducerPickerDto(x.Id, x.Code, x.Name, x.PublicStatus, x.IsVerified))
            .ToListAsync(cancellationToken);
        return TResult<PaginatedList<CatalogProducerPickerDto>>.Success(
            PaginatedList<CatalogProducerPickerDto>.Create(items, total, request.Page, request.PageSize));
    }
}
