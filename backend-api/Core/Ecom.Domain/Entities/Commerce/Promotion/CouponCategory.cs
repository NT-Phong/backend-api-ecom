namespace Ecom.Domain.Entities;
public class CouponCategory : BaseEntity
{
    public Guid CouponId { get; private set; }
    public Guid CategoryId { get; private set; }

    private CouponCategory()
    {
    }
}