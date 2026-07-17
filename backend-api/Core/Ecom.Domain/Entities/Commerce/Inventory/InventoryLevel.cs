namespace Ecom.Domain.Entities;
public class InventoryLevel : BaseEntity
{
    public Guid InventoryItemId { get; private set; }
    public Guid StockLocationId { get; private set; }
    public decimal StockedQuantity { get; private set; }
    public decimal ReservedQuantity { get; private set; }
    public decimal IncomingQuantity { get; private set; }
    public decimal AvailableQuantity => StockedQuantity - ReservedQuantity;

    public void Reserve(decimal quantity)
    {
        if (quantity <= 0 || quantity > AvailableQuantity)
            throw new ArgumentOutOfRangeException(nameof(quantity));
        ReservedQuantity += quantity;
    }

    public void Release(decimal quantity)
    {
        if (quantity <= 0 || quantity > ReservedQuantity)
            throw new ArgumentOutOfRangeException(nameof(quantity));
        ReservedQuantity -= quantity;
    }

    private InventoryLevel()
    {
    }
}