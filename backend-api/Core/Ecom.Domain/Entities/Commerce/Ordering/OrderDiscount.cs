namespace Ecom.Domain.Entities;
public class OrderDiscount : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Guid? OrderItemId { get; private set; }
    public Guid? PromotionId { get; private set; }
    public Guid? CouponId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public decimal DiscountAmount { get; private set; }

    public static OrderDiscount Create(
        Guid orderId,
        Guid? promotionId,
        Guid? couponId,
        string description,
        decimal discountAmount,
        Guid? orderItemId = null)
    {
        if (orderId == Guid.Empty)
            throw new CommerceDomainException("ORDER_DISCOUNT_ORDER_REQUIRED", "Order ID is required.");
        if (discountAmount < 0)
            throw new CommerceDomainException("ORDER_DISCOUNT_AMOUNT_INVALID", "Discount amount cannot be negative.");

        return new OrderDiscount
        {
            OrderId = orderId,
            OrderItemId = orderItemId,
            PromotionId = promotionId,
            CouponId = couponId,
            Description = description?.Trim() ?? string.Empty,
            DiscountAmount = discountAmount
        };
    }
}