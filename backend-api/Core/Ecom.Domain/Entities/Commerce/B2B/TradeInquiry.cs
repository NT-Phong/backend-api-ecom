namespace Ecom.Domain.Entities;
public class TradeInquiry : BaseEntity
{
    public string InquiryNumber { get; private set; } = string.Empty;
    public Guid? UserId { get; private set; }
    public string ContactName { get; private set; } = string.Empty;
    public string? CompanyName { get; private set; }
    public string? Email { get; private set; }
    public string PhoneNumber { get; private set; } = string.Empty;
    public TradeInquiryType InquiryType { get; private set; }
    public TradeInquiryStatus Status { get; private set; }
    public Guid? AssignedToUserId { get; private set; }
    public string? Message { get; private set; }

    private TradeInquiry()
    {
    }
}