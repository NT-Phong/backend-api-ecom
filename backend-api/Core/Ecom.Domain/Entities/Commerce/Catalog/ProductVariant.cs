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

    public static ProductVariant Create(
        Guid productId,
        string sku,
        string name,
        InventoryMode inventoryMode,
        bool allowBackorder = false,
        string? barcode = null,
        decimal? weightGrams = null,
        int displayOrder = 0)
    {
        if (productId == Guid.Empty)
            throw new CommerceDomainException("VARIANT_PRODUCT_REQUIRED", "A product is required.");
        if (string.IsNullOrWhiteSpace(sku))
            throw new CommerceDomainException("VARIANT_SKU_REQUIRED", "A SKU is required.");
        if (string.IsNullOrWhiteSpace(name))
            throw new CommerceDomainException("VARIANT_NAME_REQUIRED", "A variant name is required.");
        if (weightGrams is <= 0)
            throw new CommerceDomainException("VARIANT_WEIGHT_INVALID", "Weight must be greater than zero.");

        return new ProductVariant
        {
            ProductId = productId,
            Sku = sku.Trim(),
            Name = name.Trim(),
            Barcode = barcode?.Trim(),
            Status = VariantStatus.Active,
            InventoryMode = inventoryMode,
            AllowBackorder = allowBackorder,
            WeightGrams = weightGrams,
            DisplayOrder = displayOrder
        };
    }

    public void UpdateDetails(string name, string? barcode, decimal? weightGrams, int displayOrder)
    {
        EnsureMutable();
        if (string.IsNullOrWhiteSpace(name))
            throw new CommerceDomainException("VARIANT_NAME_REQUIRED", "A variant name is required.");
        if (weightGrams is <= 0)
            throw new CommerceDomainException("VARIANT_WEIGHT_INVALID", "Weight must be greater than zero.");

        Name = name.Trim();
        Barcode = barcode?.Trim();
        WeightGrams = weightGrams;
        DisplayOrder = displayOrder;
    }

    public void ChangeInventoryPolicy(InventoryMode inventoryMode, bool allowBackorder)
    {
        EnsureMutable();
        InventoryMode = inventoryMode;
        AllowBackorder = allowBackorder;
    }

    public void Pause() => Status = Status == VariantStatus.Discontinued
        ? throw new CommerceDomainException("VARIANT_DISCONTINUED", "A discontinued variant cannot be paused.")
        : VariantStatus.Paused;

    public void Activate() => Status = Status == VariantStatus.Discontinued
        ? throw new CommerceDomainException("VARIANT_DISCONTINUED", "A discontinued variant cannot be activated.")
        : VariantStatus.Active;

    public void Discontinue() => Status = VariantStatus.Discontinued;

    private void EnsureMutable()
    {
        if (Status == VariantStatus.Discontinued)
            throw new CommerceDomainException("VARIANT_DISCONTINUED", "A discontinued variant cannot be changed.");
    }

    private ProductVariant()
    {
    }
}
