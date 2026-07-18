namespace Ecom.Domain.Entities;
public class InquiryAttachment : BaseEntity
{
    public Guid? TradeInquiryId { get; private set; }
    public Guid? PartnerApplicationId { get; private set; }
    public Guid MediaAssetId { get; private set; }
    public MediaVisibility Visibility { get; private set; } = MediaVisibility.Internal;

    internal static InquiryAttachment CreateForTradeInquiry(Guid tradeInquiryId, Guid mediaAssetId, MediaVisibility visibility)
    {
        if (tradeInquiryId == Guid.Empty || mediaAssetId == Guid.Empty)
            throw new CommerceDomainException("INQUIRY_ATTACHMENT_REFERENCE_REQUIRED", "Trade inquiry and media asset are required.");
        if (visibility == MediaVisibility.Public)
            throw new CommerceDomainException("INQUIRY_ATTACHMENT_VISIBILITY_INVALID", "Trade inquiry attachments cannot be public.");
        return new InquiryAttachment { TradeInquiryId = tradeInquiryId, MediaAssetId = mediaAssetId, Visibility = visibility };
    }

    private InquiryAttachment()
    {
    }
}
