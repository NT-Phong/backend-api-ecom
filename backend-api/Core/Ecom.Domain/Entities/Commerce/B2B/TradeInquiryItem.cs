namespace Ecom.Domain.Entities;
public class TradeInquiryItem : BaseEntity
{
    public Guid TradeInquiryId { get; private set; }
    public Guid? ProductId { get; private set; }
    public Guid? ProductVariantId { get; private set; }
    public decimal? RequestedQuantity { get; private set; }
    public string? RequirementText { get; private set; }

    private TradeInquiryItem()
    {
    }
}