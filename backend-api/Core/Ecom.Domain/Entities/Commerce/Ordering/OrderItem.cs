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

    internal static OrderItem Create(
        Guid orderId,
        Guid? productVariantId,
        string productName,
        string variantName,
        string sku,
        decimal unitPrice,
        int quantity,
        decimal discountAmount = 0m)
    {
        if (orderId == Guid.Empty)
            throw new CommerceDomainException("ORDER_ITEM_ORDER_REQUIRED", "An order is required.");
        if (string.IsNullOrWhiteSpace(productName) || string.IsNullOrWhiteSpace(variantName) || string.IsNullOrWhiteSpace(sku))
            throw new CommerceDomainException("ORDER_ITEM_SNAPSHOT_REQUIRED", "Product, variant, and SKU snapshots are required.");
        if (unitPrice < 0 || discountAmount < 0 || quantity <= 0)
            throw new CommerceDomainException("ORDER_ITEM_AMOUNT_INVALID", "Order item amounts and quantity are invalid.");

        var gross = unitPrice * quantity;
        if (discountAmount > gross)
            throw new CommerceDomainException("ORDER_ITEM_DISCOUNT_INVALID", "Discount cannot exceed the gross line amount.");

        return new OrderItem
        {
            OrderId = orderId,
            ProductVariantId = productVariantId,
            ProductNameSnapshot = productName.Trim(),
            VariantNameSnapshot = variantName.Trim(),
            SkuSnapshot = sku.Trim(),
            UnitPriceSnapshot = unitPrice,
            Quantity = quantity,
            DiscountAmountSnapshot = discountAmount,
            LineTotalAmount = gross - discountAmount
        };
    }

    private OrderItem()
    {
    }
}
