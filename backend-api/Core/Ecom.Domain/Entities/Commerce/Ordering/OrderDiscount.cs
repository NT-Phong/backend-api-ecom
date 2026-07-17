namespace Ecom.Domain.Entities;
public class OrderDiscount : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Guid? OrderItemId { get; private set; }
    public Guid? PromotionId { get; private set; }
    public Guid? CouponId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public decimal DiscountAmount { get; private set; }

    private OrderDiscount()
    {
    }
}