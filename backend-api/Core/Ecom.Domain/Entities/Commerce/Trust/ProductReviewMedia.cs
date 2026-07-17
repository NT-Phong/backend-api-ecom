namespace Ecom.Domain.Entities;
public class ProductReviewMedia : BaseEntity
{
    public Guid ProductReviewId { get; private set; }
    public Guid MediaAssetId { get; private set; }

    private ProductReviewMedia()
    {
    }
}