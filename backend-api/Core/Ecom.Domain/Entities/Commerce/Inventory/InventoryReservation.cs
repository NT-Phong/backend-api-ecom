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

    public static InventoryReservation Create(Guid inventoryItemId, Guid stockLocationId, Guid orderItemId, decimal quantity, DateTime expiresAt)
    {
        if (inventoryItemId == Guid.Empty || stockLocationId == Guid.Empty || orderItemId == Guid.Empty)
            throw new CommerceDomainException("RESERVATION_REFERENCE_REQUIRED", "Inventory item, stock location, and order item are required.");
        if (quantity <= 0)
            throw new CommerceDomainException("RESERVATION_QUANTITY_INVALID", "Reservation quantity must be greater than zero.");
        if (expiresAt == default)
            throw new CommerceDomainException("RESERVATION_EXPIRY_REQUIRED", "Reservation expiry is required.");

        return new InventoryReservation
        {
            InventoryItemId = inventoryItemId,
            StockLocationId = stockLocationId,
            OrderItemId = orderItemId,
            Quantity = quantity,
            Status = InventoryReservationStatus.Active,
            ExpiresAt = expiresAt
        };
    }

    public void Consume()
    {
        EnsureActive();
        Status = InventoryReservationStatus.Consumed;
    }

    public void ConfirmHold()
    {
        EnsureActive();
        ExpiresAt = null;
    }

    public void Release(DateTime releasedAt)
    {
        EnsureActive();
        Status = InventoryReservationStatus.Released;
        ReleasedAt = releasedAt;
    }

    public void Expire(DateTime expiredAt)
    {
        EnsureActive();
        if (ExpiresAt is null || expiredAt < ExpiresAt.Value)
            throw new CommerceDomainException("RESERVATION_NOT_EXPIRED", "Reservation has not reached its expiry time.");
        Status = InventoryReservationStatus.Expired;
        ReleasedAt = expiredAt;
    }

    private void EnsureActive()
    {
        if (Status != InventoryReservationStatus.Active)
            throw new CommerceDomainException("RESERVATION_TERMINAL", "A terminal reservation cannot be changed.");
    }

    private InventoryReservation()
    {
    }
}
