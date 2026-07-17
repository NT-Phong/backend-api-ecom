namespace Ecom.Domain.Entities;
public class ShipmentHistory : BaseEntity
{
    public Guid ShipmentId { get; private set; }
    public ShipmentStatus? FromStatus { get; private set; }
    public ShipmentStatus ToStatus { get; private set; }
    public string? Reason { get; private set; }
    public Guid? ChangedByUserId { get; private set; }
    public DateTime OccurredAt { get; private set; }

    private ShipmentHistory()
    {
    }
}