namespace Ecom.Domain.Entities;
public class CouponRedemption : BaseEntity
{
    public Guid CouponId { get; private set; }
    public Guid? UserId { get; private set; }
    public Guid OrderId { get; private set; }
    public DateTime RedeemedAt { get; private set; }
    public decimal DiscountAmount { get; private set; }

    public static CouponRedemption Create(
        Guid couponId,
        Guid? userId,
        Guid orderId,
        decimal discountAmount,
        DateTime redeemedAt)
    {
        if (couponId == Guid.Empty)
            throw new CommerceDomainException("COUPON_REDEMPTION_COUPON_REQUIRED", "Coupon ID is required.");
        if (orderId == Guid.Empty)
            throw new CommerceDomainException("COUPON_REDEMPTION_ORDER_REQUIRED", "Order ID is required.");
        if (discountAmount < 0)
            throw new CommerceDomainException("COUPON_REDEMPTION_DISCOUNT_INVALID", "Discount amount cannot be negative.");
        if (redeemedAt == default)
            throw new CommerceDomainException("COUPON_REDEMPTION_TIME_REQUIRED", "Redemption time is required.");

        return new CouponRedemption
        {
            CouponId = couponId,
            UserId = userId,
            OrderId = orderId,
            DiscountAmount = discountAmount,
            RedeemedAt = redeemedAt
        };
    }
}