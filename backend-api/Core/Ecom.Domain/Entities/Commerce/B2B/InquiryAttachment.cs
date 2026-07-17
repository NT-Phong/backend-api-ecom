namespace Ecom.Domain.Entities;
public class InquiryAttachment : BaseEntity
{
    public Guid? TradeInquiryId { get; private set; }
    public Guid? PartnerApplicationId { get; private set; }
    public Guid MediaAssetId { get; private set; }
    public MediaVisibility Visibility { get; private set; } = MediaVisibility.Internal;

    private InquiryAttachment()
    {
    }
}