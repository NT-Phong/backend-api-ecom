using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Orders.Queries.GetManagementOrders;

public sealed record GetManagementOrdersQuery : IRequest<TResult<PaginatedList<ManagementOrderListItemDto>>>
{
    public string? Q { get; init; }
    public OrderStatus? Status { get; init; }
    public PaymentStatus? PaymentStatus { get; init; }
    public PaymentMethod? PaymentMethod { get; init; }
    public ShipmentStatus? ShipmentStatus { get; init; }
    public DateTime? FromUtc { get; init; }
    public DateTime? ToUtc { get; init; }
    public ManagementOrderSort Sort { get; init; } = ManagementOrderSort.PlacedAtDescending;
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public int Skip() => (Page - 1) * PageSize;
}

public enum ManagementOrderSort { PlacedAtDescending, PlacedAtAscending, GrandTotalDescending, GrandTotalAscending }

public sealed class GetManagementOrdersQueryValidator : AbstractValidator<GetManagementOrdersQuery>
{
    public GetManagementOrdersQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Sort).IsInEnum();
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
        if (!currentUser.HasPolicy(Permissions.Orders.Read)) return TResult<PaginatedList<ManagementOrderListItemDto>>.Failure(MessageKey.Forbidden, ErrorCodes.FORBIDDEN);

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
        if (request.PaymentMethod.HasValue)
        {
            var method = request.PaymentMethod.Value;
            orders = orders.Where(x => unitOfWork.Repository<Payment>().QueryNoTracking().Any(p => p.OrderId == x.Id && p.Method == method));
        }
        if (request.ShipmentStatus.HasValue)
        {
            var status = request.ShipmentStatus.Value;
            orders = orders.Where(x => unitOfWork.Repository<Shipment>().QueryNoTracking().Any(s => s.OrderId == x.Id && s.Status == status));
        }

        var total = await orders.CountAsync(ct);
        orders = request.Sort switch
        {
            ManagementOrderSort.PlacedAtAscending => orders.OrderBy(x => x.PlacedAt).ThenBy(x => x.Id),
            ManagementOrderSort.GrandTotalDescending => orders.OrderByDescending(x => x.GrandTotalAmount).ThenByDescending(x => x.Id),
            ManagementOrderSort.GrandTotalAscending => orders.OrderBy(x => x.GrandTotalAmount).ThenBy(x => x.Id),
            _ => orders.OrderByDescending(x => x.PlacedAt).ThenByDescending(x => x.Id)
        };
        var rows = await orders.Skip(request.Skip()).Take(request.PageSize)
            .Select(x => new { x.Id, x.OrderNumber, x.Status, x.RecipientNameSnapshot, x.RecipientPhoneSnapshot,
                x.SubtotalAmount, x.DiscountAmount, x.ShippingAmount, x.GrandTotalAmount, x.CurrencyCode, x.PlacedAt })
            .ToListAsync(ct);
        var ids = rows.Select(x => x.Id).ToList();
        var payments = await unitOfWork.Repository<Payment>().QueryNoTracking().Where(x => ids.Contains(x.OrderId))
            .ToDictionaryAsync(x => x.OrderId, x => new { x.Status, x.Method }, ct);
        var shipments = await unitOfWork.Repository<Shipment>().QueryNoTracking().Where(x => ids.Contains(x.OrderId))
            .ToDictionaryAsync(x => x.OrderId, x => (ShipmentStatus?)x.Status, ct);
        var itemCounts = await unitOfWork.Repository<OrderItem>().QueryNoTracking().Where(x => ids.Contains(x.OrderId))
            .GroupBy(x => x.OrderId).Select(x => new { OrderId = x.Key, Count = x.Count() }).ToDictionaryAsync(x => x.OrderId, x => x.Count, ct);
        var items = rows.Select(x => new ManagementOrderListItemDto(x.Id, x.OrderNumber, x.Status,
            payments.GetValueOrDefault(x.Id)?.Status ?? PaymentStatus.Pending, payments.GetValueOrDefault(x.Id)?.Method,
            shipments.GetValueOrDefault(x.Id), x.RecipientNameSnapshot, x.RecipientPhoneSnapshot,
            itemCounts.GetValueOrDefault(x.Id), x.SubtotalAmount, x.DiscountAmount, x.ShippingAmount,
            x.GrandTotalAmount, x.CurrencyCode, x.PlacedAt)).ToList();
        return TResult<PaginatedList<ManagementOrderListItemDto>>.Success(PaginatedList<ManagementOrderListItemDto>.Create(items, total, request.Page, request.PageSize));
    }
}
