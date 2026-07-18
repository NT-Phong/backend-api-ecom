namespace Ecom.Domain.Events.Commerce;

public sealed class InventoryChangedEvent : BaseEvent
{
    public Guid InventoryItemId { get; }
    public Guid StockLocationId { get; }
    public InventoryMovementType MovementType { get; }
    public decimal QuantityDelta { get; }

    public InventoryChangedEvent(Guid inventoryItemId, Guid stockLocationId, InventoryMovementType movementType, decimal quantityDelta)
    {
        InventoryItemId = inventoryItemId;
        StockLocationId = stockLocationId;
        MovementType = movementType;
        QuantityDelta = quantityDelta;
    }
}
