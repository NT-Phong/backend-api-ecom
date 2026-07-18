namespace Ecom.Domain.Entities;
public class Shipment : BaseEntity, IAggregateRoot
{
    public Guid OrderId { get; private set; }
    public string ShippingMethod { get; private set; } = string.Empty;
    public string? CarrierName { get; private set; }
    public string? TrackingCode { get; private set; }
    public ShipmentStatus Status { get; private set; }
    public DateTime? ShippedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }

    public static Shipment Create(Guid orderId, string shippingMethod, DateTime createdAt, ICollection<ShipmentHistory> history)
    {
        if (orderId == Guid.Empty || string.IsNullOrWhiteSpace(shippingMethod))
            throw new CommerceDomainException("SHIPMENT_DETAILS_REQUIRED", "Order and shipping method are required.");
        var shipment = new Shipment { OrderId = orderId, ShippingMethod = shippingMethod.Trim(), Status = ShipmentStatus.Pending };
        history.Add(ShipmentHistory.Create(shipment.Id, null, ShipmentStatus.Pending, null, null, createdAt));
        return shipment;
    }

    public void MarkReady(Guid? actorId, DateTime at, ICollection<ShipmentHistory> history) =>
        TransitionTo(ShipmentStatus.Ready, actorId, at, null, history, ShipmentStatus.Pending);

    public void StartShipping(string? carrierName, string? trackingCode, Guid? actorId, DateTime at, ICollection<ShipmentHistory> history)
    {
        CarrierName = carrierName?.Trim();
        TrackingCode = trackingCode?.Trim();
        TransitionTo(ShipmentStatus.Shipping, actorId, at, null, history, ShipmentStatus.Ready, ShipmentStatus.DeliveryFailed);
        ShippedAt = at;
    }

    public void MarkDelivered(Guid? actorId, DateTime at, ICollection<ShipmentHistory> history)
    {
        TransitionTo(ShipmentStatus.Delivered, actorId, at, null, history, ShipmentStatus.Shipping);
        DeliveredAt = at;
    }

    public void MarkDeliveryFailed(string reason, Guid? actorId, DateTime at, ICollection<ShipmentHistory> history)
    {
        EnsureReason(reason);
        TransitionTo(ShipmentStatus.DeliveryFailed, actorId, at, reason, history, ShipmentStatus.Shipping);
    }

    public void Cancel(string reason, Guid? actorId, DateTime at, ICollection<ShipmentHistory> history)
    {
        EnsureReason(reason);
        TransitionTo(ShipmentStatus.Cancelled, actorId, at, reason, history, ShipmentStatus.Pending, ShipmentStatus.Ready, ShipmentStatus.DeliveryFailed);
    }

    private void TransitionTo(ShipmentStatus target, Guid? actorId, DateTime at, string? reason, ICollection<ShipmentHistory> history, params ShipmentStatus[] allowed)
    {
        if (!allowed.Contains(Status))
            throw new CommerceDomainException("SHIPMENT_STATUS_TRANSITION_INVALID", $"Shipment cannot transition from {Status} to {target}.");
        var previous = Status;
        Status = target;
        history.Add(ShipmentHistory.Create(Id, previous, target, reason, actorId, at));
        AddDomainEvent(new CommerceStateChangedEvent(nameof(Shipment), Id, previous.ToString(), target.ToString()));
    }

    private static void EnsureReason(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new CommerceDomainException("SHIPMENT_REASON_REQUIRED", "A reason is required.");
    }

    private Shipment()
    {
    }
}
