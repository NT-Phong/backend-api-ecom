namespace Ecom.Domain.Entities;
public class Order : BaseEntity
{
    public string OrderNumber { get; private set; } = string.Empty;
    public Guid? UserId { get; private set; }
    public string? CustomerEmailSnapshot { get; private set; }
    public string CustomerPhoneSnapshot { get; private set; } = string.Empty;
    public string RecipientNameSnapshot { get; private set; } = string.Empty;
    public string RecipientPhoneSnapshot { get; private set; } = string.Empty;
    public Guid? AdministrativeAreaId { get; private set; }
    public string ShippingAddressSnapshot { get; private set; } = string.Empty;
    public OrderStatus Status { get; private set; }
    public string CurrencyCode { get; private set; } = "VND";
    public decimal SubtotalAmount { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal ShippingAmount { get; private set; }
    public decimal GrandTotalAmount { get; private set; }
    public DateTime PlacedAt { get; private set; }

    private Order()
    {
    }
}