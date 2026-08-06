using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Shipments.Commands.StartShipment;
public sealed record StartShipmentCommand(Guid OrderId, string? CarrierName, string? TrackingCode) : IRequest<TResult>, ITransactionalRequest;
public sealed class StartShipmentCommandHandler(IUnitOfWork uow, ICurrentUser current) : IRequestHandler<StartShipmentCommand, TResult>
{
    public async Task<TResult> Handle(StartShipmentCommand r, CancellationToken ct)
    {
        if (!current.HasPolicy(Permissions.Shipments.Manage)) return TResult.Failure(MessageKey.Forbidden, ErrorCodes.FORBIDDEN);
        var order = await uow.Repository<Order>().FindByIdAsync(r.OrderId); var shipment = await uow.Repository<Shipment>().Query().FirstOrDefaultAsync(x => x.OrderId == r.OrderId, ct);
        if (order is null || shipment is null) return TResult.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);
        var orderHistory = await uow.Repository<OrderStatusHistory>().Query().Where(x => x.OrderId == order.Id).ToListAsync(ct); var shipmentHistory = await uow.Repository<ShipmentHistory>().Query().Where(x => x.ShipmentId == shipment.Id).ToListAsync(ct); var now = DateTime.UtcNow;
        if (order.Status == OrderStatus.Preparing) order.StartShipping(current.UserId, now, orderHistory); else if (order.Status == OrderStatus.DeliveryFailed) order.RetryShipping(current.UserId, now, orderHistory); else return TResult.Failure("Order is not ready to ship.", ErrorCodes.UNPROCESSABLE_ENTITY);
        shipment.StartShipping(r.CarrierName, r.TrackingCode, current.UserId, now, shipmentHistory);
        var items = await uow.Repository<OrderItem>().Query().Where(x => x.OrderId == order.Id).ToListAsync(ct); var reservations = await uow.Repository<InventoryReservation>().Query().Where(x => items.Select(i => i.Id).Contains(x.OrderItemId) && x.Status == InventoryReservationStatus.Active).ToListAsync(ct);
        foreach (var reservation in reservations) { var level = await uow.Repository<InventoryLevel>().Query().FirstAsync(x => x.InventoryItemId == reservation.InventoryItemId && x.StockLocationId == reservation.StockLocationId, ct); var movement = level.Consume(reservation.Quantity, now, reservation.OrderItemId); reservation.Consume(); await uow.Repository<InventoryLevel>().UpdateAsync(level, ct); await uow.Repository<InventoryReservation>().UpdateAsync(reservation, ct); await uow.Repository<InventoryMovement>().InsertAsync(movement, ct); }
        await uow.Repository<Order>().UpdateAsync(order, ct); await uow.Repository<Shipment>().UpdateAsync(shipment, ct); await uow.Repository<OrderStatusHistory>().InsertRangeAsync(orderHistory.Where(x => x.CreatedAt == default), ct); await uow.Repository<ShipmentHistory>().InsertRangeAsync(shipmentHistory.Where(x => x.CreatedAt == default), ct);
        return TResult.Success();
    }
}
