namespace Ecom.Domain.Entities;
public class OrderItem : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Guid? ProductVariantId { get; private set; }
    public string ProductNameSnapshot { get; private set; } = string.Empty;
    public string VariantNameSnapshot { get; private set; } = string.Empty;
    public string SkuSnapshot { get; private set; } = string.Empty;
    public decimal UnitPriceSnapshot { get; private set; }
    public int Quantity { get; private set; }
    public decimal DiscountAmountSnapshot { get; private set; }
    public decimal LineTotalAmount { get; private set; }

    private OrderItem()
    {
    }
}