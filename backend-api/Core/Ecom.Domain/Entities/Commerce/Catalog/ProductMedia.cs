namespace Ecom.Domain.Entities;
public class ProductMedia : BaseEntity
{
    public Guid ProductId { get; private set; }
    public Guid MediaAssetId { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsPrimary { get; private set; }
    public string? Caption { get; private set; }

    private ProductMedia()
    {
    }
}