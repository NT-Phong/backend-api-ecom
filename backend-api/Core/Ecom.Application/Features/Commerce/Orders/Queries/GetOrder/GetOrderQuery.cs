using Ecom.Application.Common.Commerce;
using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Orders.Queries.GetOrder;

public sealed record GetOrderQuery(Guid OrderId) : IRequest<TResult<CustomerOrderDetailDto>>;

public sealed class GetOrderQueryHandler(IUnitOfWork unitOfWork, ICartPrincipalResolver principalResolver)
    : IRequestHandler<GetOrderQuery, TResult<CustomerOrderDetailDto>>
{
    public async Task<TResult<CustomerOrderDetailDto>> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        var principal = principalResolver.ResolveExistingPrincipal();
        if (principal is null) return TResult<CustomerOrderDetailDto>.Failure(MessageKey.Unauthorized, ErrorCodes.UNAUTHORIZED);

        var query = unitOfWork.Repository<Order>().QueryNoTracking()
            .Where(x => x.Id == request.OrderId);
        query = principal.UserId.HasValue
            ? query.Where(x => x.UserId == principal.UserId)
            : query.Where(x => x.UserId == null && x.GuestTokenHashSnapshot == principal.GuestTokenHash);

        var order = await query.FirstOrDefaultAsync(cancellationToken);
        if (order is null) return TResult<CustomerOrderDetailDto>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);
        var payment = await unitOfWork.Repository<Payment>().QueryNoTracking()
            .FirstOrDefaultAsync(x => x.OrderId == order.Id, cancellationToken);
        if (payment is null) return TResult<CustomerOrderDetailDto>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);

        var items = await unitOfWork.Repository<OrderItem>().QueryNoTracking()
            .Where(x => x.OrderId == order.Id)
            .OrderBy(x => x.CreatedAt)
            .Select(x => new CustomerOrderItemDto(x.Id, x.ProductVariantId, x.ProductNameSnapshot, x.VariantNameSnapshot,
                x.SkuSnapshot, x.UnitPriceSnapshot, x.Quantity, x.DiscountAmountSnapshot, x.LineTotalAmount))
            .ToListAsync(cancellationToken);
        var shipment = await unitOfWork.Repository<Shipment>().QueryNoTracking()
            .Where(x => x.OrderId == order.Id)
            .FirstOrDefaultAsync(cancellationToken);
        var orderTimeline = await unitOfWork.Repository<OrderStatusHistory>().QueryNoTracking()
            .Where(x => x.OrderId == order.Id)
            .Select(x => new CustomerOrderTimelineItemDto("order", x.ToStatus.ToString(), x.ChangedAt))
            .ToListAsync(cancellationToken);
        var shipmentTimeline = shipment is null
            ? []
            : await unitOfWork.Repository<ShipmentHistory>().QueryNoTracking()
                .Where(x => x.ShipmentId == shipment.Id)
                .Select(x => new CustomerOrderTimelineItemDto("shipment", x.ToStatus.ToString(), x.OccurredAt))
                .ToListAsync(cancellationToken);

        return TResult<CustomerOrderDetailDto>.Success(new(order.Id, order.OrderNumber, order.Status,
            order.SubtotalAmount, order.DiscountAmount, order.ShippingAmount, order.GrandTotalAmount, order.CurrencyCode,
            order.PlacedAt, order.RecipientNameSnapshot, order.RecipientPhoneSnapshot, order.ShippingAddressSnapshot, items,
            new CustomerPaymentDto(payment.Method, payment.Status, payment.Amount, payment.DueAt, payment.PaidAt), shipment is null ? null :
            new CustomerShipmentDto(shipment.Status, shipment.ShippingMethod, shipment.CarrierName, shipment.TrackingCode, shipment.ShippedAt, shipment.DeliveredAt),
            orderTimeline.Concat(shipmentTimeline).OrderBy(x => x.OccurredAt).ToList()));
    }
}
