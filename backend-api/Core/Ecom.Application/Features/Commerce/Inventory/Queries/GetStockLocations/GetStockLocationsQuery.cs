using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Inventory.Queries.GetStockLocations;

public sealed record GetStockLocationsQuery(bool? IsActive = null) : IRequest<TResult<IReadOnlyList<StockLocationDto>>>;
public sealed class GetStockLocationsQueryHandler(IUnitOfWork uow, ICurrentUser currentUser) : IRequestHandler<GetStockLocationsQuery, TResult<IReadOnlyList<StockLocationDto>>>
{
    public async Task<TResult<IReadOnlyList<StockLocationDto>>> Handle(GetStockLocationsQuery request, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated) return TResult<IReadOnlyList<StockLocationDto>>.Failure(MessageKey.Unauthorized, ErrorCodes.UNAUTHORIZED);
        if (!currentUser.HasPolicy(Permissions.Inventory.Read)) return TResult<IReadOnlyList<StockLocationDto>>.Failure(MessageKey.Forbidden, ErrorCodes.FORBIDDEN);
        var query = uow.Repository<StockLocation>().QueryNoTracking();
        if (request.IsActive.HasValue) query = query.Where(x => x.IsActive == request.IsActive.Value);
        var locations = await query.OrderBy(x => x.Code).Select(x => new StockLocationDto(x.Id, x.Code, x.Name, x.AdministrativeAreaId, x.AddressLine, x.IsActive, x.ConcurrencyStamp)).ToListAsync(ct);
        return TResult<IReadOnlyList<StockLocationDto>>.Success(locations);
    }
}
