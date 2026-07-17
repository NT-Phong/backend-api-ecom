namespace Ecom.Domain.Entities;
public class ProductVariant : BaseEntity
{
    public Guid ProductId { get; private set; }
    public string Sku { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Barcode { get; private set; }
    public VariantStatus Status { get; private set; }
    public InventoryMode InventoryMode { get; private set; }
    public bool AllowBackorder { get; private set; }
    public decimal? WeightGrams { get; private set; }
    public int DisplayOrder { get; private set; }

    private ProductVariant()
    {
    }
}