using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Orders.Queries.GetManagementOrderById;

public sealed record GetManagementOrderByIdQuery(Guid OrderId) : IRequest<TResult<ManagementOrderDetailDto>>;

public sealed class GetManagementOrderByIdQueryHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    : IRequestHandler<GetManagementOrderByIdQuery, TResult<ManagementOrderDetailDto>>
{
    public async Task<TResult<ManagementOrderDetailDto>> Handle(GetManagementOrderByIdQuery request, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated) return TResult<ManagementOrderDetailDto>.Failure(MessageKey.Unauthorized, ErrorCodes.UNAUTHORIZED);
        if (!currentUser.HasPolicy(Permissions.Orders.Manage)) return TResult<ManagementOrderDetailDto>.Failure(MessageKey.Forbidden, ErrorCodes.FORBIDDEN);
        var order = await unitOfWork.Repository<Order>().QueryNoTracking().FirstOrDefaultAsync(x => x.Id == request.OrderId, ct);
        if (order is null) return TResult<ManagementOrderDetailDto>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);
        var payment = await unitOfWork.Repository<Payment>().QueryNoTracking().FirstOrDefaultAsync(x => x.OrderId == order.Id, ct);
        if (payment is null) return TResult<ManagementOrderDetailDto>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);
        var shipment = await unitOfWork.Repository<Shipment>().QueryNoTracking().FirstOrDefaultAsync(x => x.OrderId == order.Id, ct);
        var orderItems = await unitOfWork.Repository<OrderItem>().QueryNoTracking().Where(x => x.OrderId == order.Id).OrderBy(x => x.CreatedAt)
            .Select(x => new ManagementOrderItemDto(x.Id, x.ProductVariantId, x.ProductNameSnapshot, x.VariantNameSnapshot, x.SkuSnapshot, x.UnitPriceSnapshot, x.Quantity, x.DiscountAmountSnapshot, x.LineTotalAmount)).ToListAsync(ct);
        var notes = await unitOfWork.Repository<OrderNote>().QueryNoTracking().Where(x => x.OrderId == order.Id).OrderByDescending(x => x.CreatedAt)
            .Select(x => new ManagementOrderNoteDto(x.Id, x.AuthorUserId, x.NoteType, x.Content, x.IsVisibleToCustomer, x.CreatedAt)).ToListAsync(ct);
        var orderTimeline = await unitOfWork.Repository<OrderStatusHistory>().QueryNoTracking().Where(x => x.OrderId == order.Id)
            .Select(x => new ManagementOrderTimelineItemDto("order", x.ToStatus.ToString(), x.Reason, x.ChangedByUserId, x.ChangedAt)).ToListAsync(ct);
        var shipmentTimeline = shipment is null ? [] : await unitOfWork.Repository<ShipmentHistory>().QueryNoTracking().Where(x => x.ShipmentId == shipment.Id)
            .Select(x => new ManagementOrderTimelineItemDto("shipment", x.ToStatus.ToString(), x.Reason, x.ChangedByUserId, x.OccurredAt)).ToListAsync(ct);
        return TResult<ManagementOrderDetailDto>.Success(new(order.Id, order.OrderNumber, order.Status, order.CustomerEmailSnapshot,
            order.CustomerPhoneSnapshot, order.RecipientNameSnapshot, order.RecipientPhoneSnapshot, order.ShippingAddressSnapshot,
            order.SubtotalAmount, order.DiscountAmount, order.ShippingAmount, order.GrandTotalAmount, order.CurrencyCode, order.PlacedAt,
            new CustomerPaymentDto(payment.Method, payment.Status, payment.Amount, payment.DueAt, payment.PaidAt), shipment is null ? null :
            new CustomerShipmentDto(shipment.Status, shipment.ShippingMethod, shipment.CarrierName, shipment.TrackingCode, shipment.ShippedAt, shipment.DeliveredAt),
            orderItems, notes, orderTimeline.Concat(shipmentTimeline).OrderBy(x => x.OccurredAt).ToList()));
    }
}
