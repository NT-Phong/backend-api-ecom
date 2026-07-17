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

    private InventoryMovement()
    {
    }
}