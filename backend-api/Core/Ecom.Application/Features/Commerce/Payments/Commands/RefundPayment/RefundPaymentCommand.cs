using Ecom.Domain.Entities;
using Ecom.Application.Common.Commerce;

namespace Ecom.Application.Features.Commerce.Payments.Commands.RefundPayment;

public sealed record RefundPaymentCommand(Guid OrderId, string ProviderReference, string Reason)
    : IRequest<TResult>, ITransactionalRequest;

public sealed class RefundPaymentCommandValidator : AbstractValidator<RefundPaymentCommand>
{
    public RefundPaymentCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.ProviderReference).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}

public sealed class RefundPaymentCommandHandler(IUnitOfWork unitOfWork, ICurrentUser current,
    IInventoryReservationStore inventoryStore) : IRequestHandler<RefundPaymentCommand, TResult>
{
    public async Task<TResult> Handle(RefundPaymentCommand request, CancellationToken cancellationToken)
    {
        if (!current.HasPolicy(Permissions.Payments.Refund))
            return TResult.Failure(MessageKey.Forbidden, ErrorCodes.FORBIDDEN);

        var order = await unitOfWork.Repository<Order>().FindByIdAsync(request.OrderId);
        var payment = await unitOfWork.Repository<Payment>().Query()
            .FirstOrDefaultAsync(x => x.OrderId == request.OrderId, cancellationToken);
        if (order is null || payment is null)
            return TResult.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);
        if (payment.Status != PaymentStatus.Paid)
            return TResult.Failure("Only a paid order can be refunded.", ErrorCodes.UNPROCESSABLE_ENTITY);

        var items = await unitOfWork.Repository<OrderItem>().Query().Where(x => x.OrderId == order.Id)
            .ToListAsync(cancellationToken);
        var reservations = await unitOfWork.Repository<InventoryReservation>().Query()
            .Where(x => items.Select(item => item.Id).Contains(x.OrderItemId)
                        && x.Status == InventoryReservationStatus.Active)
            .ToListAsync(cancellationToken);
        var lockResult = await inventoryStore.LockInventoryLevelsAsync(reservations
            .Select(x => new InventoryLevelLockRequest(x.InventoryItemId, x.StockLocationId)).ToList(), cancellationToken);
        if (!lockResult.IsSuccess)
            return TResult.Failure(lockResult.Error!, lockResult.ErrorCode);

        var now = DateTime.UtcNow;
        var history = await unitOfWork.Repository<OrderStatusHistory>().Query().Where(x => x.OrderId == order.Id)
            .ToListAsync(cancellationToken);
        order.Cancel(request.Reason, current.UserId == Guid.Empty ? null : current.UserId, now, history);

        foreach (var reservation in reservations)
        {
            var key = new InventoryLevelLockRequest(reservation.InventoryItemId, reservation.StockLocationId);
            var level = lockResult.Data[key];
            var movement = level.Release(reservation.Quantity, now, reservation.OrderItemId, request.Reason);
            reservation.Release(now);
            await unitOfWork.Repository<InventoryLevel>().UpdateAsync(level, cancellationToken);
            await unitOfWork.Repository<InventoryReservation>().UpdateAsync(reservation, cancellationToken);
            await unitOfWork.Repository<InventoryMovement>().InsertAsync(movement, cancellationToken);
        }

        var refund = payment.Refund(payment.Amount, "manual-refund", request.ProviderReference, now);
        await unitOfWork.Repository<Order>().UpdateAsync(order, cancellationToken);
        await unitOfWork.Repository<OrderStatusHistory>().InsertRangeAsync(history.Where(x => x.CreatedAt == default), cancellationToken);
        await unitOfWork.Repository<Payment>().UpdateAsync(payment, cancellationToken);
        await unitOfWork.Repository<PaymentTransaction>().InsertAsync(refund, cancellationToken);
        return TResult.Success();
    }
}
