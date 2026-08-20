using Ecom.Application.Common.Commerce;
using Ecom.Application.Common.Interfaces;
using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Inventory.Commands.ExpireReservations;

public sealed record ExpireReservationsCommand : IRequest<TResult>, ITransactionalRequest;

public sealed class ExpireReservationsCommandHandler(
    IUnitOfWork unitOfWork,
    IInventoryReservationStore inventoryReservationStore,
    IOrderLifecycleStore orderLifecycleStore)
    : IRequestHandler<ExpireReservationsCommand, TResult>
{
    public async Task<TResult> Handle(ExpireReservationsCommand request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var expiredReservations = await unitOfWork.Repository<InventoryReservation>()
            .Query()
            .Where(x => x.Status == InventoryReservationStatus.Active && x.ExpiresAt != null && x.ExpiresAt <= now)
            .ToListAsync(cancellationToken);

        if (expiredReservations.Count == 0)
            return TResult.Success();

        var expiredReservationIds = expiredReservations.Select(x => x.Id).ToHashSet();
        var expiredOrderItemIds = expiredReservations.Select(x => x.OrderItemId).Distinct().ToArray();
        var expiredOrderIds = await unitOfWork.Repository<OrderItem>()
            .QueryNoTracking()
            .Where(x => expiredOrderItemIds.Contains(x.Id))
            .Select(x => x.OrderId)
            .Distinct()
            .ToListAsync(cancellationToken);

        foreach (var orderId in expiredOrderIds)
        {
            // Keep the lifecycle lock order aligned with cancellation and confirmation.
            var order = await orderLifecycleStore.LockOrderAsync(orderId, cancellationToken);
            var payment = await orderLifecycleStore.LockPaymentAsync(orderId, cancellationToken);
            if (order is null || payment is null || payment.Status == PaymentStatus.Paid || order.Status != OrderStatus.Pending)
                continue;

            var orderItemIds = await unitOfWork.Repository<OrderItem>()
                .QueryNoTracking()
                .Where(x => x.OrderId == orderId)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);
            var activeReservations = await unitOfWork.Repository<InventoryReservation>()
                .Query()
                .Where(x => orderItemIds.Contains(x.OrderItemId) && x.Status == InventoryReservationStatus.Active)
                .ToListAsync(cancellationToken);

            if (activeReservations.Count == 0)
                continue;

            var levelLocks = await inventoryReservationStore.LockInventoryLevelsAsync(
                activeReservations
                    .Select(x => new InventoryLevelLockRequest(x.InventoryItemId, x.StockLocationId))
                    .ToList(),
                cancellationToken);
            if (!levelLocks.IsSuccess)
                return TResult.Failure(levelLocks.Error!, levelLocks.ErrorCode);

            foreach (var reservation in activeReservations)
            {
                var levelKey = new InventoryLevelLockRequest(reservation.InventoryItemId, reservation.StockLocationId);
                var level = levelLocks.Data[levelKey];
                var isExpired = expiredReservationIds.Contains(reservation.Id) ||
                    (reservation.ExpiresAt is not null && reservation.ExpiresAt <= now);
                var reason = isExpired ? "reservation-expired" : "order-expired-reservation-release";

                var movement = level.Release(reservation.Quantity, now, reservation.OrderItemId, reason);
                if (isExpired)
                    reservation.Expire(now);
                else
                    reservation.Release(now);

                await unitOfWork.Repository<InventoryLevel>().UpdateAsync(level, cancellationToken);
                await unitOfWork.Repository<InventoryReservation>().UpdateAsync(reservation, cancellationToken);
                await unitOfWork.Repository<InventoryMovement>().InsertAsync(movement, cancellationToken);
            }

            var history = await unitOfWork.Repository<OrderStatusHistory>()
                .Query()
                .Where(x => x.OrderId == order.Id)
                .ToListAsync(cancellationToken);
            order.Cancel("Reservation expired before confirmation.", null, now, history);
            var transaction = payment.Cancel("reservation-expiry", now);

            await unitOfWork.Repository<Order>().UpdateAsync(order, cancellationToken);
            await unitOfWork.Repository<OrderStatusHistory>().InsertRangeAsync(
                history.Where(x => x.CreatedAt == default), cancellationToken);
            await unitOfWork.Repository<Payment>().UpdateAsync(payment, cancellationToken);
            await unitOfWork.Repository<PaymentTransaction>().InsertAsync(transaction, cancellationToken);
        }

        return TResult.Success();
    }
}
