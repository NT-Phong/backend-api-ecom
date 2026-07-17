namespace Ecom.Domain.Entities;
public class CouponRedemption : BaseEntity
{
    public Guid CouponId { get; private set; }
    public Guid? UserId { get; private set; }
    public Guid OrderId { get; private set; }
    public DateTime RedeemedAt { get; private set; }
    public decimal DiscountAmount { get; private set; }

    private CouponRedemption()
    {
    }
}