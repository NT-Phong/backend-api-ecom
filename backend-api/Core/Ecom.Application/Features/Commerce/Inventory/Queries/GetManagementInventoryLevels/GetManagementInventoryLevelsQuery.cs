using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Inventory.Queries.GetManagementInventoryLevels;

public sealed record GetManagementInventoryLevelsQuery : IRequest<TResult<PaginatedList<InventoryLevelDto>>>
{
    public Guid? StockLocationId { get; init; }
    public string? Q { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public int Skip() => (Page - 1) * PageSize;
}
public sealed class GetManagementInventoryLevelsQueryValidator : AbstractValidator<GetManagementInventoryLevelsQuery>
{ public GetManagementInventoryLevelsQueryValidator() { RuleFor(x => x.Page).GreaterThan(0); RuleFor(x => x.PageSize).InclusiveBetween(1, 100); } }
public sealed class GetManagementInventoryLevelsQueryHandler(IUnitOfWork uow, ICurrentUser currentUser)
    : IRequestHandler<GetManagementInventoryLevelsQuery, TResult<PaginatedList<InventoryLevelDto>>>
{
    public async Task<TResult<PaginatedList<InventoryLevelDto>>> Handle(GetManagementInventoryLevelsQuery request, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated) return TResult<PaginatedList<InventoryLevelDto>>.Failure(MessageKey.Unauthorized, ErrorCodes.UNAUTHORIZED);
        if (!currentUser.HasPolicy(Permissions.Inventory.Read)) return TResult<PaginatedList<InventoryLevelDto>>.Failure(MessageKey.Forbidden, ErrorCodes.FORBIDDEN);
        var query = from level in uow.Repository<InventoryLevel>().QueryNoTracking()
                    join item in uow.Repository<InventoryItem>().QueryNoTracking() on level.InventoryItemId equals item.Id
                    join variant in uow.Repository<ProductVariant>().QueryNoTracking() on item.ProductVariantId equals variant.Id
                    join product in uow.Repository<Product>().QueryNoTracking() on variant.ProductId equals product.Id
                    join location in uow.Repository<StockLocation>().QueryNoTracking() on level.StockLocationId equals location.Id
                    select new { level, item, variant, product, location };
        if (request.StockLocationId.HasValue) query = query.Where(x => x.level.StockLocationId == request.StockLocationId.Value);
        if (!string.IsNullOrWhiteSpace(request.Q)) { var q = request.Q.Trim(); query = query.Where(x => x.variant.Sku.Contains(q) || x.product.Name.Contains(q) || x.location.Code.Contains(q)); }
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.product.Name).ThenBy(x => x.variant.Sku).Skip(request.Skip()).Take(request.PageSize)
            .Select(x => new InventoryLevelDto(x.item.Id, x.item.ProductVariantId, x.variant.Sku, x.product.Name, x.variant.Name,
                x.location.Id, x.location.Code, x.level.StockedQuantity, x.level.ReservedQuantity, x.level.IncomingQuantity, x.level.AvailableQuantity)).ToListAsync(ct);
        return TResult<PaginatedList<InventoryLevelDto>>.Success(PaginatedList<InventoryLevelDto>.Create(rows, total, request.Page, request.PageSize));
    }
}
