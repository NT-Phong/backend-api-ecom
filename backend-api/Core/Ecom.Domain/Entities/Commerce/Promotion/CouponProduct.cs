namespace Ecom.Domain.Entities;
public class CouponProduct : BaseEntity
{
    public Guid CouponId { get; private set; }
    public Guid ProductId { get; private set; }

    public static CouponProduct Create(Guid couponId, Guid productId)
    {
        if (couponId == Guid.Empty || productId == Guid.Empty)
            throw new CommerceDomainException("COUPON_PRODUCT_INVALID", "Coupon ID and product ID are required.");
        return new CouponProduct { CouponId = couponId, ProductId = productId };
    }
}