namespace Ecom.Domain.Entities;
public class CouponProduct : BaseEntity
{
    public Guid CouponId { get; private set; }
    public Guid ProductId { get; private set; }

    private CouponProduct()
    {
    }
}