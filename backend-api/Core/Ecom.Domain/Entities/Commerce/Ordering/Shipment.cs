namespace Ecom.Domain.Entities;
public class Shipment : BaseEntity
{
    public Guid OrderId { get; private set; }
    public string ShippingMethod { get; private set; } = string.Empty;
    public string? CarrierName { get; private set; }
    public string? TrackingCode { get; private set; }
    public ShipmentStatus Status { get; private set; }
    public DateTime? ShippedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }

    private Shipment()
    {
    }
}