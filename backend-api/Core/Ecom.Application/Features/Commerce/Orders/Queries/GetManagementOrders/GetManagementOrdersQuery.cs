using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Orders.Queries.GetManagementOrders;

public sealed record GetManagementOrdersQuery : IRequest<TResult<PaginatedList<ManagementOrderListItemDto>>>
{
    public string? Q { get; init; }
    public OrderStatus? Status { get; init; }
    public PaymentStatus? PaymentStatus { get; init; }
    public DateTime? FromUtc { get; init; }
    public DateTime? ToUtc { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public int Skip() => (Page - 1) * PageSize;
}

public sealed class GetManagementOrdersQueryValidator : AbstractValidator<GetManagementOrdersQuery>
{
    public GetManagementOrdersQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x).Must(x => !x.FromUtc.HasValue || !x.ToUtc.HasValue || x.FromUtc <= x.ToUtc)
            .WithMessage("FromUtc must be earlier than or equal to ToUtc.");
    }
}

public sealed class GetManagementOrdersQueryHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    : IRequestHandler<GetManagementOrdersQuery, TResult<PaginatedList<ManagementOrderListItemDto>>>
{
    public async Task<TResult<PaginatedList<ManagementOrderListItemDto>>> Handle(GetManagementOrdersQuery request, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated) return TResult<PaginatedList<ManagementOrderListItemDto>>.Failure(MessageKey.Unauthorized, ErrorCodes.UNAUTHORIZED);
        if (!currentUser.HasPolicy(Permissions.Orders.Manage)) return TResult<PaginatedList<ManagementOrderListItemDto>>.Failure(MessageKey.Forbidden, ErrorCodes.FORBIDDEN);

        var orders = unitOfWork.Repository<Order>().QueryNoTracking();
        if (!string.IsNullOrWhiteSpace(request.Q))
        {
            var q = request.Q.Trim();
            orders = orders.Where(x => x.OrderNumber.Contains(q) || x.RecipientNameSnapshot.Contains(q) || x.RecipientPhoneSnapshot.Contains(q));
        }
        if (request.Status.HasValue) orders = orders.Where(x => x.Status == request.Status.Value);
        if (request.FromUtc.HasValue) orders = orders.Where(x => x.PlacedAt >= request.FromUtc.Value);
        if (request.ToUtc.HasValue) orders = orders.Where(x => x.PlacedAt <= request.ToUtc.Value);
        if (request.PaymentStatus.HasValue)
        {
            var status = request.PaymentStatus.Value;
            orders = orders.Where(x => unitOfWork.Repository<Payment>().QueryNoTracking().Any(p => p.OrderId == x.Id && p.Status == status));
        }

        var total = await orders.CountAsync(ct);
        var rows = await orders.OrderByDescending(x => x.PlacedAt).ThenByDescending(x => x.Id).Skip(request.Skip()).Take(request.PageSize)
            .Select(x => new { x.Id, x.OrderNumber, x.Status, x.RecipientNameSnapshot, x.RecipientPhoneSnapshot, x.GrandTotalAmount, x.CurrencyCode, x.PlacedAt })
            .ToListAsync(ct);
        var ids = rows.Select(x => x.Id).ToList();
        var payments = await unitOfWork.Repository<Payment>().QueryNoTracking().Where(x => ids.Contains(x.OrderId))
            .ToDictionaryAsync(x => x.OrderId, x => x.Status, ct);
        var shipments = await unitOfWork.Repository<Shipment>().QueryNoTracking().Where(x => ids.Contains(x.OrderId))
            .ToDictionaryAsync(x => x.OrderId, x => (ShipmentStatus?)x.Status, ct);
        var items = rows.Select(x => new ManagementOrderListItemDto(x.Id, x.OrderNumber, x.Status,
            payments.GetValueOrDefault(x.Id, PaymentStatus.Pending), shipments.GetValueOrDefault(x.Id), x.RecipientNameSnapshot,
            x.RecipientPhoneSnapshot, x.GrandTotalAmount, x.CurrencyCode, x.PlacedAt)).ToList();
        return TResult<PaginatedList<ManagementOrderListItemDto>>.Success(PaginatedList<ManagementOrderListItemDto>.Create(items, total, request.Page, request.PageSize));
    }
}
