namespace Ecom.Domain.Entities;
public class ShipmentHistory : BaseEntity
{
    public Guid ShipmentId { get; private set; }
    public ShipmentStatus? FromStatus { get; private set; }
    public ShipmentStatus ToStatus { get; private set; }
    public string? Reason { get; private set; }
    public Guid? ChangedByUserId { get; private set; }
    public DateTime OccurredAt { get; private set; }

    internal static ShipmentHistory Create(Guid shipmentId, ShipmentStatus? from, ShipmentStatus to, string? reason, Guid? actorId, DateTime occurredAt) =>
        new()
        {
            ShipmentId = shipmentId,
            FromStatus = from,
            ToStatus = to,
            Reason = reason?.Trim(),
            ChangedByUserId = actorId,
            OccurredAt = occurredAt
        };

    private ShipmentHistory()
    {
    }
}
