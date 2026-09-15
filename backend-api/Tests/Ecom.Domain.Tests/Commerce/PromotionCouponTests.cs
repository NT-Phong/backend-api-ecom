namespace Ecom.Domain.Tests.Commerce;

public class PromotionCouponTests
{
    [Fact]
    public void Promotion_lifecycle_and_discount_calculation()
    {
        var now = DateTime.UtcNow;
        var promotion = Promotion.Create(
            "SUMMER2026",
            "Khuyến mãi hè 2026",
            PromotionType.Percentage,
            10m, // 10%
            now.AddDays(-1),
            now.AddDays(7),
            100_000m, // min spend 100k
            PromotionStatus.Active);

        Assert.True(promotion.IsActiveAt(now));
        Assert.False(promotion.IsActiveAt(now.AddDays(-2)));
        Assert.False(promotion.IsActiveAt(now.AddDays(8)));

        // Min spend not reached
        var belowMinSpend = promotion.CalculateDiscount(80_000m, 30_000m);
        Assert.Equal(0m, belowMinSpend);

        // 10% discount on 500k = 50k
        var discount = promotion.CalculateDiscount(500_000m, 30_000m);
        Assert.Equal(50_000m, discount);

        // Fixed discount promotion
        var fixedPromo = Promotion.Create(
            "GIAM30K",
            "Giảm 30k cho đơn từ 100k",
            PromotionType.FixedAmount,
            30_000m,
            now.AddDays(-1),
            now.AddDays(7),
            100_000m,
            PromotionStatus.Active);

        var fixedDiscount = fixedPromo.CalculateDiscount(200_000m, 30_000m);
        Assert.Equal(30_000m, fixedDiscount);

        // Free shipping promotion
        var freeShipPromo = Promotion.Create(
            "FREESHIP",
            "Miễn phí vận chuyển",
            PromotionType.FreeShipping,
            0m,
            now.AddDays(-1),
            now.AddDays(7),
            null,
            PromotionStatus.Active);

        var freeShipDiscount = freeShipPromo.CalculateDiscount(200_000m, 25_000m);
        Assert.Equal(25_000m, freeShipDiscount);
        Assert.Equal(0m, freeShipPromo.CalculateDiscount(200_000m, 0m));

        // State transitions
        promotion.Pause();
        Assert.Equal(PromotionStatus.Paused, promotion.Status);
        Assert.False(promotion.IsActiveAt(now));

        promotion.Activate();
        Assert.Equal(PromotionStatus.Active, promotion.Status);

        promotion.Expire();
        Assert.Equal(PromotionStatus.Expired, promotion.Status);
    }

    [Fact]
    public void Coupon_lifecycle_eligibility_and_invariants()
    {
        var now = DateTime.UtcNow;
        var promotionId = Guid.NewGuid();

        var coupon = Coupon.Create(
            "HE2026",
            promotionId,
            100, // usage limit
            2,   // per user limit
            now.AddDays(-1),
            now.AddDays(7),
            CouponStatus.Active);

        Assert.Equal("HE2026", coupon.Code);
        Assert.True(coupon.IsEligible(now));
        Assert.False(coupon.IsEligible(now.AddDays(-2)));
        Assert.False(coupon.IsEligible(now.AddDays(8)));

        // State transition to Paused
        coupon.Pause();
        Assert.Equal(CouponStatus.Paused, coupon.Status);
        Assert.False(coupon.IsEligible(now));

        coupon.Activate();
        Assert.Equal(CouponStatus.Active, coupon.Status);

        coupon.Expire();
        Assert.Equal(CouponStatus.Expired, coupon.Status);
        Assert.False(coupon.IsEligible(now));
    }

    [Fact]
    public void Order_ApplyOrderDiscount_adjusts_discount_and_grand_total()
    {
        var items = new List<OrderItem>();
        var history = new List<OrderStatusHistory>();
        var now = DateTime.UtcNow;
        var order = Order.Create(
            "ORD-DISC-001",
            null,
            "guest-hash",
            null,
            "0900000000",
            "Buyer",
            "0900000000",
            null,
            "Address",
            30_000m,
            now,
            [new OrderLineSnapshot(Guid.NewGuid(), "Product", "Variant", "SKU-1", 100_000m, 2, 20_000m)],
            items,
            history);

        // Initial: Subtotal = 200k, Line Discount = 20k, Shipping = 30k -> GrandTotal = 210k
        Assert.Equal(200_000m, order.SubtotalAmount);
        Assert.Equal(20_000m, order.DiscountAmount);
        Assert.Equal(210_000m, order.GrandTotalAmount);

        // Apply coupon discount 50k
        order.ApplyOrderDiscount(50_000m);

        Assert.Equal(70_000m, order.DiscountAmount);
        Assert.Equal(160_000m, order.GrandTotalAmount);

        // Applying negative discount must fail
        Assert.Throws<CommerceDomainException>(() => order.ApplyOrderDiscount(-10_000m));
    }

    [Fact]
    public void CouponRedemption_and_OrderDiscount_validation()
    {
        var now = DateTime.UtcNow;
        var couponId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var promotionId = Guid.NewGuid();

        // Valid CouponRedemption
        var redemption = CouponRedemption.Create(couponId, null, orderId, 25_000m, now);
        Assert.Equal(couponId, redemption.CouponId);
        Assert.Equal(orderId, redemption.OrderId);
        Assert.Equal(25_000m, redemption.DiscountAmount);

        var zeroValueRedemption = CouponRedemption.Create(couponId, null, Guid.NewGuid(), 0m, now);
        Assert.Equal(0m, zeroValueRedemption.DiscountAmount);

        // Invalid CouponRedemption
        Assert.Throws<CommerceDomainException>(() => CouponRedemption.Create(Guid.Empty, null, orderId, 10_000m, now));
        Assert.Throws<CommerceDomainException>(() => CouponRedemption.Create(couponId, null, Guid.Empty, 10_000m, now));
        Assert.Throws<CommerceDomainException>(() => CouponRedemption.Create(couponId, null, orderId, -5_000m, now));

        // Valid OrderDiscount
        var orderDiscount = OrderDiscount.Create(orderId, promotionId, couponId, "Voucher 50k", 50_000m);
        Assert.Equal("Voucher 50k", orderDiscount.Description);
        Assert.Equal(50_000m, orderDiscount.DiscountAmount);

        // Invalid OrderDiscount
        Assert.Throws<CommerceDomainException>(() => OrderDiscount.Create(Guid.Empty, promotionId, couponId, "Desc", 10_000m));
        Assert.Throws<CommerceDomainException>(() => OrderDiscount.Create(orderId, promotionId, couponId, "Desc", -10_000m));
    }
}
