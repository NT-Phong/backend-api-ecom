namespace Ecom.Domain.Entities;
public class Coupon : BaseEntity
{
    public Guid? PromotionId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public int? UsageLimit { get; private set; }
    public int? PerUserLimit { get; private set; }
    public DateTime StartsAt { get; private set; }
    public DateTime? EndsAt { get; private set; }
    public CouponStatus Status { get; private set; }

    private Coupon()
    {
    }
}