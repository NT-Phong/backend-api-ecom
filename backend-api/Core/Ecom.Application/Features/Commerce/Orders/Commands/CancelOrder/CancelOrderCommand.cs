using Ecom.Application.Common.Commerce;
using Ecom.Application.Common.Interfaces;
using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Orders.Commands.CancelOrder;
public sealed record CancelOrderCommand(Guid OrderId, string Reason) : IRequest<TResult>, ITransactionalRequest;
public sealed class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand> { public CancelOrderCommandValidator() { RuleFor(x => x.OrderId).NotEmpty(); RuleFor(x => x.Reason).NotEmpty().MaximumLength(500); } }
public sealed class CancelOrderCommandHandler(
    IUnitOfWork uow,
    ICurrentUser current,
    ICartPrincipalResolver principalResolver,
    IInventoryReservationStore inventoryReservationStore,
    IOrderLifecycleStore orderLifecycleStore)
    : IRequestHandler<CancelOrderCommand, TResult>
{
    public async Task<TResult> Handle(CancelOrderCommand r, CancellationToken ct)
    {
        var order = await orderLifecycleStore.LockOrderAsync(r.OrderId, ct); if (order is null) return TResult.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);
        var staff = current.HasPolicy(Permissions.Orders.Manage);
        if (!staff)
        {
            var principal = principalResolver.ResolveExistingPrincipal();
            var ownsOrder = principal is not null &&
                (principal.UserId.HasValue
                    ? order.UserId == principal.UserId
                    : order.UserId is null && order.GuestTokenHashSnapshot == principal.GuestTokenHash);
            if (!ownsOrder) return TResult.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);
        }
        var payment = await orderLifecycleStore.LockPaymentAsync(order.Id, ct);
        if (payment is null || payment.Status == PaymentStatus.Paid) return TResult.Failure("A paid order requires recorded refund before cancellation.", ErrorCodes.UNPROCESSABLE_ENTITY);
        var now = DateTime.UtcNow;
        var items = await uow.Repository<OrderItem>().Query().Where(x => x.OrderId == order.Id).ToListAsync(ct);
        var reservations = await uow.Repository<InventoryReservation>().Query().Where(x => items.Select(i => i.Id).Contains(x.OrderItemId) && x.Status == InventoryReservationStatus.Active).ToListAsync(ct);
        var levelLocks = await inventoryReservationStore.LockInventoryLevelsAsync(
            reservations.Select(x => new InventoryLevelLockRequest(x.InventoryItemId, x.StockLocationId)).ToList(), ct);
        if (!levelLocks.IsSuccess)
            return TResult.Failure(levelLocks.Error!, levelLocks.ErrorCode);

        var history = await uow.Repository<OrderStatusHistory>().Query().Where(x => x.OrderId == order.Id).ToListAsync(ct);
        order.Cancel(r.Reason, current.UserId == Guid.Empty ? null : current.UserId, now, history);
        foreach (var reservation in reservations)
        {
            var level = levelLocks.Data[new InventoryLevelLockRequest(reservation.InventoryItemId, reservation.StockLocationId)];
            var movement = level.Release(reservation.Quantity, now, reservation.OrderItemId, r.Reason); reservation.Release(now);
            await uow.Repository<InventoryLevel>().UpdateAsync(level, ct); await uow.Repository<InventoryReservation>().UpdateAsync(reservation, ct); await uow.Repository<InventoryMovement>().InsertAsync(movement, ct);
        }
        var cancellation = payment.Cancel("order-cancellation", DateTime.UtcNow);
        await uow.Repository<Payment>().UpdateAsync(payment, ct); await uow.Repository<PaymentTransaction>().InsertAsync(cancellation, ct);
        await uow.Repository<Order>().UpdateAsync(order, ct); await uow.Repository<OrderStatusHistory>().InsertRangeAsync(history.Where(x => x.CreatedAt == default), ct);
        return TResult.Success();
    }
}
