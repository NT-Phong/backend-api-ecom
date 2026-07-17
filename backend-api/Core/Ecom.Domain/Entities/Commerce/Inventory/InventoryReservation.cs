namespace Ecom.Domain.Entities;
public class InventoryReservation : BaseEntity
{
    public Guid InventoryItemId { get; private set; }
    public Guid StockLocationId { get; private set; }
    public Guid OrderItemId { get; private set; }
    public decimal Quantity { get; private set; }
    public InventoryReservationStatus Status { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public DateTime? ReleasedAt { get; private set; }

    private InventoryReservation()
    {
    }
}