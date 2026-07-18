namespace Ecom.Domain.Entities;
public class InventoryItem : BaseEntity, IAggregateRoot
{
    public Guid ProductVariantId { get; private set; }
    public bool RequiresShipping { get; private set; } = true;

    public static InventoryItem Create(Guid productVariantId, bool requiresShipping = true)
    {
        if (productVariantId == Guid.Empty)
            throw new CommerceDomainException("INVENTORY_VARIANT_REQUIRED", "A product variant is required.");
        return new InventoryItem { ProductVariantId = productVariantId, RequiresShipping = requiresShipping };
    }

    public void SetRequiresShipping(bool requiresShipping) => RequiresShipping = requiresShipping;

    private InventoryItem()
    {
    }
}
