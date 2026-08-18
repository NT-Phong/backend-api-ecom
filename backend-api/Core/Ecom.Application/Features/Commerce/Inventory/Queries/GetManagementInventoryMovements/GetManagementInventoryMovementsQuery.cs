using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Inventory.Queries.GetManagementInventoryMovements;

public sealed record GetManagementInventoryMovementsQuery : IRequest<TResult<PaginatedList<InventoryMovementDto>>>
{
    public Guid? InventoryItemId { get; init; }
    public Guid? StockLocationId { get; init; }
    public InventoryMovementType? MovementType { get; init; }
    public DateTime? FromUtc { get; init; }
    public DateTime? ToUtc { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
    public int Skip() => (Page - 1) * PageSize;
}
public sealed class GetManagementInventoryMovementsQueryValidator : AbstractValidator<GetManagementInventoryMovementsQuery>
{ public GetManagementInventoryMovementsQueryValidator() { RuleFor(x => x.Page).GreaterThan(0); RuleFor(x => x.PageSize).InclusiveBetween(1, 100); RuleFor(x => x).Must(x => !x.FromUtc.HasValue || !x.ToUtc.HasValue || x.FromUtc <= x.ToUtc).WithMessage("FromUtc must be earlier than or equal to ToUtc."); } }
public sealed class GetManagementInventoryMovementsQueryHandler(IUnitOfWork uow, ICurrentUser currentUser)
    : IRequestHandler<GetManagementInventoryMovementsQuery, TResult<PaginatedList<InventoryMovementDto>>>
{
    public async Task<TResult<PaginatedList<InventoryMovementDto>>> Handle(GetManagementInventoryMovementsQuery request, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated) return TResult<PaginatedList<InventoryMovementDto>>.Failure(MessageKey.Unauthorized, ErrorCodes.UNAUTHORIZED);
        if (!currentUser.HasPolicy(Permissions.Inventory.Read)) return TResult<PaginatedList<InventoryMovementDto>>.Failure(MessageKey.Forbidden, ErrorCodes.FORBIDDEN);
        var query = uow.Repository<InventoryMovement>().QueryNoTracking();
        if (request.InventoryItemId.HasValue) query = query.Where(x => x.InventoryItemId == request.InventoryItemId.Value);
        if (request.StockLocationId.HasValue) query = query.Where(x => x.StockLocationId == request.StockLocationId.Value);
        if (request.MovementType.HasValue) query = query.Where(x => x.MovementType == request.MovementType.Value);
        if (request.FromUtc.HasValue) query = query.Where(x => x.OccurredAt >= request.FromUtc.Value);
        if (request.ToUtc.HasValue) query = query.Where(x => x.OccurredAt <= request.ToUtc.Value);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderByDescending(x => x.OccurredAt).ThenByDescending(x => x.Id).Skip(request.Skip()).Take(request.PageSize)
            .Select(x => new InventoryMovementDto(x.Id, x.InventoryItemId, x.StockLocationId, x.OrderItemId, x.MovementType, x.QuantityDelta, x.Reason, x.OccurredAt)).ToListAsync(ct);
        return TResult<PaginatedList<InventoryMovementDto>>.Success(PaginatedList<InventoryMovementDto>.Create(rows, total, request.Page, request.PageSize));
    }
}
