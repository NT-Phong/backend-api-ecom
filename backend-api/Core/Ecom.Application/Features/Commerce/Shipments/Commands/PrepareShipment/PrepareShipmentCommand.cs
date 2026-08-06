using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Shipments.Commands.PrepareShipment;
public sealed record PrepareShipmentCommand(Guid OrderId) : IRequest<TResult>, ITransactionalRequest;
public sealed class PrepareShipmentCommandHandler(IUnitOfWork uow, ICurrentUser current) : IRequestHandler<PrepareShipmentCommand, TResult>
{
    public async Task<TResult> Handle(PrepareShipmentCommand r, CancellationToken ct)
    {
        if (!current.HasPolicy(Permissions.Shipments.Manage)) return TResult.Failure(MessageKey.Forbidden, ErrorCodes.FORBIDDEN);
        var order = await uow.Repository<Order>().FindByIdAsync(r.OrderId); var shipment = await uow.Repository<Shipment>().Query().FirstOrDefaultAsync(x => x.OrderId == r.OrderId, ct);
        if (order is null || shipment is null) return TResult.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);
        var orderHistory = await uow.Repository<OrderStatusHistory>().Query().Where(x => x.OrderId == order.Id).ToListAsync(ct); var shipmentHistory = await uow.Repository<ShipmentHistory>().Query().Where(x => x.ShipmentId == shipment.Id).ToListAsync(ct);
        var now = DateTime.UtcNow; order.StartPreparing(current.UserId, now, orderHistory); shipment.MarkReady(current.UserId, now, shipmentHistory);
        await uow.Repository<Order>().UpdateAsync(order, ct); await uow.Repository<Shipment>().UpdateAsync(shipment, ct);
        await uow.Repository<OrderStatusHistory>().InsertRangeAsync(orderHistory.Where(x => x.CreatedAt == default), ct); await uow.Repository<ShipmentHistory>().InsertRangeAsync(shipmentHistory.Where(x => x.CreatedAt == default), ct);
        return TResult.Success();
    }
}
