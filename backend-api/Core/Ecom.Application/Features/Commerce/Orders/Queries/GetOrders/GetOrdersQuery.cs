using Ecom.Application.Common.Commerce;
using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Orders.Queries.GetOrders;

public sealed record GetOrdersQuery : IRequest<TResult<IReadOnlyList<OrderSummaryDto>>>;

public sealed class GetOrdersQueryHandler(IUnitOfWork unitOfWork, ICartPrincipalResolver principalResolver)
    : IRequestHandler<GetOrdersQuery, TResult<IReadOnlyList<OrderSummaryDto>>>
{
    public async Task<TResult<IReadOnlyList<OrderSummaryDto>>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var principal = principalResolver.ResolveExistingPrincipal();
        if (principal is null) return TResult<IReadOnlyList<OrderSummaryDto>>.Failure(MessageKey.Unauthorized, ErrorCodes.UNAUTHORIZED);

        var orders = unitOfWork.Repository<Order>().QueryNoTracking();
        orders = principal.UserId.HasValue
            ? orders.Where(x => x.UserId == principal.UserId)
            : orders.Where(x => x.UserId == null && x.GuestTokenHashSnapshot == principal.GuestTokenHash);
        var rows = await orders.OrderByDescending(x => x.PlacedAt)
            .Select(x => new { x.Id, x.OrderNumber, x.Status, x.GrandTotalAmount, x.PlacedAt })
            .ToListAsync(cancellationToken);
        var paymentStatuses = await unitOfWork.Repository<Payment>().QueryNoTracking()
            .Where(x => rows.Select(order => order.Id).Contains(x.OrderId))
            .ToDictionaryAsync(x => x.OrderId, x => x.Status, cancellationToken);

        return TResult<IReadOnlyList<OrderSummaryDto>>.Success(rows
            .Select(x => new OrderSummaryDto(x.Id, x.OrderNumber, x.Status,
                paymentStatuses.GetValueOrDefault(x.Id, PaymentStatus.Pending), x.GrandTotalAmount, x.PlacedAt))
            .ToList());
    }
}
