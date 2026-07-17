namespace Ecom.Domain.Entities;
public class InventoryItem : BaseEntity
{
    public Guid ProductVariantId { get; private set; }
    public bool RequiresShipping { get; private set; } = true;

    private InventoryItem()
    {
    }
}