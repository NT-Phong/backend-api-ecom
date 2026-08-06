using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Shipments.Commands.MarkDeliveryFailed;

public sealed record MarkDeliveryFailedCommand(Guid OrderId, string Reason) : IRequest<TResult>, ITransactionalRequest;

public sealed class MarkDeliveryFailedCommandValidator : AbstractValidator<MarkDeliveryFailedCommand>
{
    public MarkDeliveryFailedCommandValidator() => RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
}

public sealed class MarkDeliveryFailedCommandHandler(IUnitOfWork uow, ICurrentUser current)
    : IRequestHandler<MarkDeliveryFailedCommand, TResult>
{
    public async Task<TResult> Handle(MarkDeliveryFailedCommand request, CancellationToken cancellationToken)
    {
        if (!current.HasPolicy(Permissions.Shipments.Manage)) return TResult.Failure(MessageKey.Forbidden, ErrorCodes.FORBIDDEN);
        var order = await uow.Repository<Order>().FindByIdAsync(request.OrderId);
        var shipment = await uow.Repository<Shipment>().Query().FirstOrDefaultAsync(x => x.OrderId == request.OrderId, cancellationToken);
        if (order is null || shipment is null) return TResult.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);

        var orderHistory = await uow.Repository<OrderStatusHistory>().Query().Where(x => x.OrderId == order.Id).ToListAsync(cancellationToken);
        var shipmentHistory = await uow.Repository<ShipmentHistory>().Query().Where(x => x.ShipmentId == shipment.Id).ToListAsync(cancellationToken);
        var now = DateTime.UtcNow;
        order.MarkDeliveryFailed(request.Reason, current.UserId, now, orderHistory);
        shipment.MarkDeliveryFailed(request.Reason, current.UserId, now, shipmentHistory);
        await uow.Repository<Order>().UpdateAsync(order, cancellationToken);
        await uow.Repository<Shipment>().UpdateAsync(shipment, cancellationToken);
        await uow.Repository<OrderStatusHistory>().InsertRangeAsync(orderHistory.Where(x => x.CreatedAt == default), cancellationToken);
        await uow.Repository<ShipmentHistory>().InsertRangeAsync(shipmentHistory.Where(x => x.CreatedAt == default), cancellationToken);
        return TResult.Success();
    }
}
