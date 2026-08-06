using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Shipments.Commands.CompleteShipment;
public sealed record CompleteShipmentCommand(Guid OrderId) : IRequest<TResult>, ITransactionalRequest;

public sealed class CompleteShipmentCommandValidator : AbstractValidator<CompleteShipmentCommand>
{
    public CompleteShipmentCommandValidator() => RuleFor(x => x.OrderId).NotEmpty();
}

public sealed class CompleteShipmentCommandHandler(IUnitOfWork uow, ICurrentUser current, IOrderLifecycleStore orderLifecycleStore) : IRequestHandler<CompleteShipmentCommand, TResult>
{
    public async Task<TResult> Handle(CompleteShipmentCommand r, CancellationToken ct)
    {
        if (!current.HasPolicy(Permissions.Shipments.Manage)) return TResult.Failure(MessageKey.Forbidden, ErrorCodes.FORBIDDEN);
        var order = await orderLifecycleStore.LockOrderAsync(r.OrderId, ct); var shipment = await orderLifecycleStore.LockShipmentAsync(r.OrderId, ct); var payment = await orderLifecycleStore.LockPaymentAsync(r.OrderId, ct);
        if (order is null || shipment is null || payment is null) return TResult.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);
        var orderHistory = await uow.Repository<OrderStatusHistory>().Query().Where(x => x.OrderId == order.Id).ToListAsync(ct); var shipmentHistory = await uow.Repository<ShipmentHistory>().Query().Where(x => x.ShipmentId == shipment.Id).ToListAsync(ct); var now = DateTime.UtcNow;
        shipment.MarkDelivered(current.UserId, now, shipmentHistory); order.Complete(current.UserId, now, orderHistory);
        if (payment.Method == PaymentMethod.COD && payment.Status == PaymentStatus.Pending) { var transaction = payment.MarkPaid(payment.Amount, "cod", order.OrderNumber, now); await uow.Repository<PaymentTransaction>().InsertAsync(transaction, ct); await uow.Repository<Payment>().UpdateAsync(payment, ct); }
        await uow.Repository<Order>().UpdateAsync(order, ct); await uow.Repository<Shipment>().UpdateAsync(shipment, ct); await uow.Repository<OrderStatusHistory>().InsertRangeAsync(orderHistory.Where(x => x.CreatedAt == default), ct); await uow.Repository<ShipmentHistory>().InsertRangeAsync(shipmentHistory.Where(x => x.CreatedAt == default), ct);
        return TResult.Success();
    }
}
