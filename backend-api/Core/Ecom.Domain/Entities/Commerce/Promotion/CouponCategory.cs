namespace Ecom.Domain.Entities;
public class CouponCategory : BaseEntity
{
    public Guid CouponId { get; private set; }
    public Guid CategoryId { get; private set; }

    public static CouponCategory Create(Guid couponId, Guid categoryId)
    {
        if (couponId == Guid.Empty || categoryId == Guid.Empty)
            throw new CommerceDomainException("COUPON_CATEGORY_INVALID", "Coupon ID and category ID are required.");
        return new CouponCategory { CouponId = couponId, CategoryId = categoryId };
    }
}