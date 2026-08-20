namespace Ecom.Domain.Entities;
public class InventoryLevel : BaseEntity
{
    public Guid InventoryItemId { get; private set; }
    public Guid StockLocationId { get; private set; }
    public decimal StockedQuantity { get; private set; }
    public decimal ReservedQuantity { get; private set; }
    public decimal IncomingQuantity { get; private set; }
    public decimal AvailableQuantity => StockedQuantity - ReservedQuantity;

    public static InventoryLevel Create(Guid inventoryItemId, Guid stockLocationId)
    {
        if (inventoryItemId == Guid.Empty || stockLocationId == Guid.Empty)
            throw new CommerceDomainException("INVENTORY_LEVEL_REFERENCE_REQUIRED", "Inventory item and stock location are required.");
        return new InventoryLevel { InventoryItemId = inventoryItemId, StockLocationId = stockLocationId };
    }

    public InventoryMovement Receive(decimal quantity, DateTime occurredAt, string? reason = null)
    {
        EnsurePositive(quantity);
        StockedQuantity += quantity;
        AddDomainEvent(new InventoryChangedEvent(InventoryItemId, StockLocationId, InventoryMovementType.Receive, quantity));
        return InventoryMovement.Create(InventoryItemId, StockLocationId, InventoryMovementType.Receive, quantity, occurredAt, reason: reason);
    }

    public void Reserve(decimal quantity)
    {
        if (quantity <= 0 || quantity > AvailableQuantity)
            throw new CommerceDomainException("INVENTORY_INSUFFICIENT", "Available inventory is insufficient.");
        ReservedQuantity += quantity;
        AddDomainEvent(new InventoryChangedEvent(InventoryItemId, StockLocationId, InventoryMovementType.Allocate, quantity));
    }

    public InventoryMovement Reserve(decimal quantity, DateTime occurredAt, Guid orderItemId)
    {
        if (orderItemId == Guid.Empty)
            throw new CommerceDomainException("INVENTORY_RESERVATION_ORDER_ITEM_REQUIRED", "An order item is required.");
        Reserve(quantity);
        return InventoryMovement.Create(InventoryItemId, StockLocationId, InventoryMovementType.Allocate,
            quantity, occurredAt, orderItemId);
    }

    public void Release(decimal quantity)
    {
        if (quantity <= 0 || quantity > ReservedQuantity)
            throw new CommerceDomainException("INVENTORY_RELEASE_INVALID", "Release quantity exceeds reserved inventory.");
        ReservedQuantity -= quantity;
        AddDomainEvent(new InventoryChangedEvent(InventoryItemId, StockLocationId, InventoryMovementType.Release, quantity));
    }

    public InventoryMovement Release(decimal quantity, DateTime occurredAt, Guid? orderItemId, string? reason)
    {
        Release(quantity);
        return InventoryMovement.Create(InventoryItemId, StockLocationId, InventoryMovementType.Release, quantity, occurredAt, orderItemId, reason);
    }

    public InventoryMovement Consume(decimal quantity, DateTime occurredAt, Guid orderItemId)
    {
        EnsurePositive(quantity);
        if (quantity > ReservedQuantity || quantity > StockedQuantity)
            throw new CommerceDomainException("INVENTORY_CONSUME_INVALID", "Consume quantity exceeds stocked or reserved inventory.");
        ReservedQuantity -= quantity;
        StockedQuantity -= quantity;
        AddDomainEvent(new InventoryChangedEvent(InventoryItemId, StockLocationId, InventoryMovementType.Ship, -quantity));
        return InventoryMovement.Create(InventoryItemId, StockLocationId, InventoryMovementType.Ship, -quantity, occurredAt, orderItemId);
    }

    public InventoryMovement ReceiveReturn(decimal quantity, DateTime occurredAt, Guid orderItemId, string reason)
    {
        EnsurePositive(quantity);
        if (orderItemId == Guid.Empty || string.IsNullOrWhiteSpace(reason))
            throw new CommerceDomainException("INVENTORY_RETURN_DETAILS_REQUIRED", "An order item and return reason are required.");
        StockedQuantity += quantity;
        AddDomainEvent(new InventoryChangedEvent(InventoryItemId, StockLocationId, InventoryMovementType.Return, quantity));
        return InventoryMovement.Create(InventoryItemId, StockLocationId, InventoryMovementType.Return, quantity, occurredAt,
            orderItemId, reason);
    }

    public InventoryMovement Adjust(decimal quantityDelta, DateTime occurredAt, string reason)
    {
        if (quantityDelta == 0 || string.IsNullOrWhiteSpace(reason))
            throw new CommerceDomainException("INVENTORY_ADJUSTMENT_INVALID", "A non-zero adjustment and reason are required.");
        if (StockedQuantity + quantityDelta < ReservedQuantity)
            throw new CommerceDomainException("INVENTORY_ADJUSTMENT_BELOW_RESERVED", "Adjustment cannot reduce stock below reserved quantity.");
        StockedQuantity += quantityDelta;
        AddDomainEvent(new InventoryChangedEvent(InventoryItemId, StockLocationId, InventoryMovementType.Adjust, quantityDelta));
        return InventoryMovement.Create(InventoryItemId, StockLocationId, InventoryMovementType.Adjust, quantityDelta, occurredAt, reason: reason);
    }

    private static void EnsurePositive(decimal quantity)
    {
        if (quantity <= 0)
            throw new CommerceDomainException("INVENTORY_QUANTITY_INVALID", "Inventory quantity must be greater than zero.");
    }

    private InventoryLevel()
    {
    }
}
