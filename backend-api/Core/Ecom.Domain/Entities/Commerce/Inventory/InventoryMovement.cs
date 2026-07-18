namespace Ecom.Domain.Entities;
public class InventoryMovement : BaseEntity
{
    public Guid InventoryItemId { get; private set; }
    public Guid StockLocationId { get; private set; }
    public Guid? OrderItemId { get; private set; }
    public InventoryMovementType MovementType { get; private set; }
    public decimal QuantityDelta { get; private set; }
    public string? Reason { get; private set; }
    public DateTime OccurredAt { get; private set; }

    internal static InventoryMovement Create(
        Guid inventoryItemId,
        Guid stockLocationId,
        InventoryMovementType type,
        decimal quantityDelta,
        DateTime occurredAt,
        Guid? orderItemId = null,
        string? reason = null)
    {
        if (inventoryItemId == Guid.Empty || stockLocationId == Guid.Empty)
            throw new CommerceDomainException("INVENTORY_MOVEMENT_REFERENCE_REQUIRED", "Inventory item and stock location are required.");
        if (quantityDelta == 0)
            throw new CommerceDomainException("INVENTORY_MOVEMENT_QUANTITY_INVALID", "Movement quantity cannot be zero.");
        if (occurredAt == default)
            throw new CommerceDomainException("INVENTORY_MOVEMENT_TIME_REQUIRED", "Movement time is required.");

        return new InventoryMovement
        {
            InventoryItemId = inventoryItemId,
            StockLocationId = stockLocationId,
            OrderItemId = orderItemId,
            MovementType = type,
            QuantityDelta = quantityDelta,
            Reason = reason?.Trim(),
            OccurredAt = occurredAt
        };
    }

    private InventoryMovement()
    {
    }
}
