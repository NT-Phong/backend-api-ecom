namespace Ecom.Domain.Entities;
public class ProductMedia : BaseEntity
{
    public Guid ProductId { get; private set; }
    public Guid MediaAssetId { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsPrimary { get; private set; }
    public string? Caption { get; private set; }

    internal static ProductMedia Create(Guid productId, Guid mediaAssetId, int displayOrder, bool isPrimary, string? caption)
    {
        if (productId == Guid.Empty || mediaAssetId == Guid.Empty)
            throw new CommerceDomainException("PRODUCT_MEDIA_REFERENCE_REQUIRED", "Product and media asset are required.");
        if (displayOrder < 0)
            throw new CommerceDomainException("PRODUCT_MEDIA_ORDER_INVALID", "Media display order cannot be negative.");

        return new ProductMedia
        {
            ProductId = productId,
            MediaAssetId = mediaAssetId,
            DisplayOrder = displayOrder,
            IsPrimary = isPrimary,
            Caption = caption?.Trim()
        };
    }

    internal void SetPrimary(bool isPrimary) => IsPrimary = isPrimary;

    internal void Reorder(int displayOrder)
    {
        if (displayOrder < 0)
            throw new CommerceDomainException("PRODUCT_MEDIA_ORDER_INVALID", "Media display order cannot be negative.");
        DisplayOrder = displayOrder;
    }

    internal void UpdateCaption(string? caption) => Caption = caption?.Trim();

    private ProductMedia()
    {
    }
}
