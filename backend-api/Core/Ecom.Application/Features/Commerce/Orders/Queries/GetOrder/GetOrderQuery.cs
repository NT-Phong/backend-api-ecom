using Ecom.Application.Common.Commerce;
using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Orders.Queries.GetOrder;

public sealed record GetOrderQuery(Guid OrderId) : IRequest<TResult<OrderSummaryDto>>;

public sealed class GetOrderQueryHandler(IUnitOfWork unitOfWork, ICartPrincipalResolver principalResolver)
    : IRequestHandler<GetOrderQuery, TResult<OrderSummaryDto>>
{
    public async Task<TResult<OrderSummaryDto>> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        var principal = principalResolver.ResolveExistingPrincipal();
        if (principal is null) return TResult<OrderSummaryDto>.Failure(MessageKey.Unauthorized, ErrorCodes.UNAUTHORIZED);

        var query = unitOfWork.Repository<Order>().QueryNoTracking()
            .Where(x => x.Id == request.OrderId);
        query = principal.UserId.HasValue
            ? query.Where(x => x.UserId == principal.UserId)
            : query.Where(x => x.UserId == null && x.GuestTokenHashSnapshot == principal.GuestTokenHash);

        var order = await query.FirstOrDefaultAsync(cancellationToken);
        if (order is null) return TResult<OrderSummaryDto>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);
        var payment = await unitOfWork.Repository<Payment>().QueryNoTracking()
            .FirstOrDefaultAsync(x => x.OrderId == order.Id, cancellationToken);
        if (payment is null) return TResult<OrderSummaryDto>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);

        return TResult<OrderSummaryDto>.Success(new(order.Id, order.OrderNumber, order.Status, payment.Status,
            order.GrandTotalAmount, order.PlacedAt));
    }
}
