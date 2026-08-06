using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Orders.Commands.CancelOrder;
public sealed record CancelOrderCommand(Guid OrderId, string Reason) : IRequest<TResult>, ITransactionalRequest;
public sealed class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand> { public CancelOrderCommandValidator() { RuleFor(x => x.OrderId).NotEmpty(); RuleFor(x => x.Reason).NotEmpty().MaximumLength(500); } }
public sealed class CancelOrderCommandHandler(IUnitOfWork uow, ICurrentUser current) : IRequestHandler<CancelOrderCommand, TResult>
{
    public async Task<TResult> Handle(CancelOrderCommand r, CancellationToken ct)
    {
        var order = await uow.Repository<Order>().FindByIdAsync(r.OrderId); if (order is null) return TResult.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);
        var staff = current.HasPolicy(Permissions.Orders.Manage);
        if (!staff && (!current.IsAuthenticated || order.UserId != current.UserId)) return TResult.Failure(MessageKey.Forbidden, ErrorCodes.FORBIDDEN);
        var payment = await uow.Repository<Payment>().Query().FirstOrDefaultAsync(x => x.OrderId == order.Id, ct);
        if (payment is null || payment.Status == PaymentStatus.Paid) return TResult.Failure("A paid order requires recorded refund before cancellation.", ErrorCodes.UNPROCESSABLE_ENTITY);
        var history = await uow.Repository<OrderStatusHistory>().Query().Where(x => x.OrderId == order.Id).ToListAsync(ct);
        order.Cancel(r.Reason, current.UserId == Guid.Empty ? null : current.UserId, DateTime.UtcNow, history);
        var items = await uow.Repository<OrderItem>().Query().Where(x => x.OrderId == order.Id).ToListAsync(ct);
        var reservations = await uow.Repository<InventoryReservation>().Query().Where(x => items.Select(i => i.Id).Contains(x.OrderItemId) && x.Status == InventoryReservationStatus.Active).ToListAsync(ct);
        foreach (var reservation in reservations)
        {
            var level = await uow.Repository<InventoryLevel>().Query().FirstAsync(x => x.InventoryItemId == reservation.InventoryItemId && x.StockLocationId == reservation.StockLocationId, ct);
            var movement = level.Release(reservation.Quantity, DateTime.UtcNow, reservation.OrderItemId, r.Reason); reservation.Release(DateTime.UtcNow);
            await uow.Repository<InventoryLevel>().UpdateAsync(level, ct); await uow.Repository<InventoryReservation>().UpdateAsync(reservation, ct); await uow.Repository<InventoryMovement>().InsertAsync(movement, ct);
        }
        var cancellation = payment.Cancel("order-cancellation", DateTime.UtcNow);
        await uow.Repository<Payment>().UpdateAsync(payment, ct); await uow.Repository<PaymentTransaction>().InsertAsync(cancellation, ct);
        await uow.Repository<Order>().UpdateAsync(order, ct); await uow.Repository<OrderStatusHistory>().InsertRangeAsync(history.Where(x => x.CreatedAt == default), ct);
        return TResult.Success();
    }
}
