using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Orders.Commands.ConfirmOrder;
public sealed record ConfirmOrderCommand(Guid OrderId) : IRequest<TResult>, ITransactionalRequest;

public sealed class ConfirmOrderCommandValidator : AbstractValidator<ConfirmOrderCommand>
{
    public ConfirmOrderCommandValidator() => RuleFor(x => x.OrderId).NotEmpty();
}

public sealed class ConfirmOrderCommandHandler(IUnitOfWork uow, ICurrentUser current, IOrderLifecycleStore orderLifecycleStore) : IRequestHandler<ConfirmOrderCommand, TResult>
{
    public async Task<TResult> Handle(ConfirmOrderCommand r, CancellationToken ct)
    {
        if (!current.HasPolicy(Permissions.Orders.Manage)) return TResult.Failure(MessageKey.Forbidden, ErrorCodes.FORBIDDEN);
        var order = await orderLifecycleStore.LockOrderAsync(r.OrderId, ct); if (order is null) return TResult.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);
        var payment = await orderLifecycleStore.LockPaymentAsync(order.Id, ct);
        if (payment is null || (payment.Method == PaymentMethod.BankTransfer && payment.Status != PaymentStatus.Paid)) return TResult.Failure("Payment must be verified before confirmation.", ErrorCodes.UNPROCESSABLE_ENTITY);
        var history = await uow.Repository<OrderStatusHistory>().Query().Where(x => x.OrderId == order.Id).ToListAsync(ct);
        order.Confirm(current.UserId, DateTime.UtcNow, history);
        var items = await uow.Repository<OrderItem>().Query().Where(x => x.OrderId == order.Id).ToListAsync(ct);
        var reservations = await uow.Repository<InventoryReservation>().Query().Where(x => items.Select(i => i.Id).Contains(x.OrderItemId) && x.Status == InventoryReservationStatus.Active).ToListAsync(ct);
        foreach (var reservation in reservations) { reservation.ConfirmHold(); await uow.Repository<InventoryReservation>().UpdateAsync(reservation, ct); }
        var shipmentHistory = new List<ShipmentHistory>(); var shipment = Shipment.Create(order.Id, "standard", DateTime.UtcNow, shipmentHistory);
        await uow.Repository<Shipment>().InsertAsync(shipment, ct); await uow.Repository<ShipmentHistory>().InsertRangeAsync(shipmentHistory, ct);
        await uow.Repository<Order>().UpdateAsync(order, ct); await uow.Repository<OrderStatusHistory>().InsertRangeAsync(history.Where(x => x.CreatedAt == default), ct);
        return TResult.Success();
    }
}
