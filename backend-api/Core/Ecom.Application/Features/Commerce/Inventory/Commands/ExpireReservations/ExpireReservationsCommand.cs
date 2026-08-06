using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Inventory.Commands.ExpireReservations;
public sealed record ExpireReservationsCommand : IRequest<TResult>, ITransactionalRequest;
public sealed class ExpireReservationsCommandHandler(IUnitOfWork uow) : IRequestHandler<ExpireReservationsCommand, TResult>
{
    public async Task<TResult> Handle(ExpireReservationsCommand request, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var expired = await uow.Repository<InventoryReservation>().Query().Where(x => x.Status == InventoryReservationStatus.Active && x.ExpiresAt != null && x.ExpiresAt <= now).ToListAsync(ct);
        foreach (var reservation in expired)
        {
            var orderItem = await uow.Repository<OrderItem>().FindByIdAsync(reservation.OrderItemId);
            var order = orderItem is null ? null : await uow.Repository<Order>().FindByIdAsync(orderItem.OrderId);
            var payment = order is null ? null : await uow.Repository<Payment>().FindOneAsync([x => x.OrderId == order.Id]);
            if (order is null || payment is null || payment.Status == PaymentStatus.Paid || order.Status != OrderStatus.Pending) continue;
            var level = await uow.Repository<InventoryLevel>().Query().FirstAsync(x => x.InventoryItemId == reservation.InventoryItemId && x.StockLocationId == reservation.StockLocationId, ct);
            var movement = level.Release(reservation.Quantity, now, reservation.OrderItemId, "reservation-expired"); reservation.Expire(now);
            var history = await uow.Repository<OrderStatusHistory>().Query().Where(x => x.OrderId == order.Id).ToListAsync(ct);
            order.Cancel("Reservation expired before confirmation.", null, now, history);
            var transaction = payment.Cancel("reservation-expiry", now);
            await uow.Repository<InventoryLevel>().UpdateAsync(level, ct); await uow.Repository<InventoryReservation>().UpdateAsync(reservation, ct); await uow.Repository<InventoryMovement>().InsertAsync(movement, ct);
            await uow.Repository<Order>().UpdateAsync(order, ct); await uow.Repository<OrderStatusHistory>().InsertRangeAsync(history.Where(x => x.CreatedAt == default), ct);
            await uow.Repository<Payment>().UpdateAsync(payment, ct); await uow.Repository<PaymentTransaction>().InsertAsync(transaction, ct);
        }
        return TResult.Success();
    }
}
