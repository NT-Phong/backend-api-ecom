using Ecom.Application.Common.Commerce;
using Ecom.Application.Common.Interfaces;
using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Inventory.Commands.ReceiveReturnedOrderItems;

public sealed record ReturnedOrderItemRequest(Guid OrderItemId, int Quantity);

public sealed record ReceiveReturnedOrderItemsCommand(Guid OrderId, IReadOnlyList<ReturnedOrderItemRequest> Items, string Reason)
    : IRequest<TResult<IReadOnlyList<InventoryMovementDto>>>, ITransactionalRequest;

public sealed class ReceiveReturnedOrderItemsCommandValidator : AbstractValidator<ReceiveReturnedOrderItemsCommand>
{
    public ReceiveReturnedOrderItemsCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.OrderItemId).NotEmpty();
            item.RuleFor(x => x.Quantity).InclusiveBetween(1, 999);
        });
        RuleFor(x => x.Items.Select(item => item.OrderItemId)).Must(ids => ids.Distinct().Count() == ids.Count())
            .WithMessage("Each order item may appear only once.");
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
    }
}

public sealed class ReceiveReturnedOrderItemsCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IOrderLifecycleStore orderLifecycleStore,
    IInventoryReservationStore inventoryStore)
    : IRequestHandler<ReceiveReturnedOrderItemsCommand, TResult<IReadOnlyList<InventoryMovementDto>>>
{
    public async Task<TResult<IReadOnlyList<InventoryMovementDto>>> Handle(ReceiveReturnedOrderItemsCommand request,
        CancellationToken cancellationToken)
    {
        if (!currentUser.HasPolicy(Permissions.Inventory.Adjust))
            return TResult<IReadOnlyList<InventoryMovementDto>>.Failure(MessageKey.Forbidden, ErrorCodes.FORBIDDEN);

        var order = await orderLifecycleStore.LockOrderAsync(request.OrderId, cancellationToken);
        var shipment = await orderLifecycleStore.LockShipmentAsync(request.OrderId, cancellationToken);
        if (order is null || shipment is null)
            return TResult<IReadOnlyList<InventoryMovementDto>>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);
        if (order.Status is not (OrderStatus.DeliveryFailed or OrderStatus.Cancelled)
            || shipment.Status != ShipmentStatus.DeliveryFailed)
            return TResult<IReadOnlyList<InventoryMovementDto>>.Failure(
                "Inventory can be returned only for a failed delivery.", ErrorCodes.UNPROCESSABLE_ENTITY);

        var itemIds = request.Items.Select(item => item.OrderItemId).ToArray();
        var orderItems = await unitOfWork.Repository<OrderItem>().QueryNoTracking()
            .Where(item => item.OrderId == order.Id && itemIds.Contains(item.Id)).ToListAsync(cancellationToken);
        if (orderItems.Count != itemIds.Length)
            return TResult<IReadOnlyList<InventoryMovementDto>>.Failure("One or more order items were not found.", ErrorCodes.NOT_FOUND);

        var reservations = await unitOfWork.Repository<InventoryReservation>().QueryNoTracking()
            .Where(reservation => itemIds.Contains(reservation.OrderItemId) && reservation.Status == InventoryReservationStatus.Consumed)
            .ToListAsync(cancellationToken);
        if (reservations.Count != itemIds.Length)
            return TResult<IReadOnlyList<InventoryMovementDto>>.Failure(
                "One or more order items have not been shipped from tracked inventory.", ErrorCodes.UNPROCESSABLE_ENTITY);

        var reservationByOrderItemId = reservations.ToDictionary(reservation => reservation.OrderItemId);
        var locks = await inventoryStore.LockInventoryLevelsAsync(reservations
            .Select(reservation => new InventoryLevelLockRequest(reservation.InventoryItemId, reservation.StockLocationId)).ToList(), cancellationToken);
        if (!locks.IsSuccess)
            return TResult<IReadOnlyList<InventoryMovementDto>>.Failure(locks.Error!, locks.ErrorCode);

        // The inventory-level locks serialize two staff requests that try to receive the same failed-delivery item.
        var returnedQuantities = await unitOfWork.Repository<InventoryMovement>().QueryNoTracking()
            .Where(movement => movement.OrderItemId != null && itemIds.Contains(movement.OrderItemId.Value)
                               && movement.MovementType == InventoryMovementType.Return)
            .GroupBy(movement => movement.OrderItemId!.Value)
            .Select(group => new { OrderItemId = group.Key, Quantity = group.Sum(x => x.QuantityDelta) })
            .ToDictionaryAsync(x => x.OrderItemId, x => x.Quantity, cancellationToken);

        var orderItemById = orderItems.ToDictionary(item => item.Id);
        foreach (var item in request.Items)
            if (returnedQuantities.GetValueOrDefault(item.OrderItemId) + item.Quantity > orderItemById[item.OrderItemId].Quantity)
                return TResult<IReadOnlyList<InventoryMovementDto>>.Failure(
                    "Returned quantity cannot exceed the shipped order-item quantity.", ErrorCodes.UNPROCESSABLE_ENTITY);

        var now = DateTime.UtcNow;
        var movements = new List<InventoryMovement>();
        foreach (var item in request.Items)
        {
            var reservation = reservationByOrderItemId[item.OrderItemId];
            var key = new InventoryLevelLockRequest(reservation.InventoryItemId, reservation.StockLocationId);
            var level = locks.Data[key];
            var movement = level.ReceiveReturn(item.Quantity, now, item.OrderItemId, request.Reason);
            movements.Add(movement);
            await unitOfWork.Repository<InventoryLevel>().UpdateAsync(level, cancellationToken);
            await unitOfWork.Repository<InventoryMovement>().InsertAsync(movement, cancellationToken);
        }

        return TResult<IReadOnlyList<InventoryMovementDto>>.Success(movements.Select(movement => new InventoryMovementDto(
            movement.Id, movement.InventoryItemId, movement.StockLocationId, movement.OrderItemId, movement.MovementType,
            movement.QuantityDelta, movement.Reason, movement.OccurredAt)).ToList());
    }
}
