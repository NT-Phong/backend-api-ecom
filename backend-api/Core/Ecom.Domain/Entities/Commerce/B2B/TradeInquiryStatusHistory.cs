namespace Ecom.Domain.Entities;
public class TradeInquiryStatusHistory : BaseEntity
{
    public Guid TradeInquiryId { get; private set; }
    public TradeInquiryStatus? FromStatus { get; private set; }
    public TradeInquiryStatus ToStatus { get; private set; }
    public string? Reason { get; private set; }
    public Guid? ChangedByUserId { get; private set; }
    public DateTime ChangedAt { get; private set; }

    internal static TradeInquiryStatusHistory Create(Guid inquiryId, TradeInquiryStatus? from, TradeInquiryStatus to,
        string? reason, Guid? actorId, DateTime changedAt) => new()
        {
            TradeInquiryId = inquiryId,
            FromStatus = from,
            ToStatus = to,
            Reason = reason?.Trim(),
            ChangedByUserId = actorId,
            ChangedAt = changedAt
        };

    private TradeInquiryStatusHistory()
    {
    }
}
