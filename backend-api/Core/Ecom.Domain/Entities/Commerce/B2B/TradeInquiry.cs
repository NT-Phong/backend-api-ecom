namespace Ecom.Domain.Entities;
public class TradeInquiry : BaseEntity, IAggregateRoot
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

    public static TradeInquiry Create(string inquiryNumber, Guid? userId, string contactName, string? companyName,
        string? email, string phoneNumber, TradeInquiryType type, string? message, DateTime createdAt,
        ICollection<TradeInquiryStatusHistory> history)
    {
        if (string.IsNullOrWhiteSpace(inquiryNumber) || string.IsNullOrWhiteSpace(contactName) || string.IsNullOrWhiteSpace(phoneNumber))
            throw new CommerceDomainException("TRADE_INQUIRY_DETAILS_REQUIRED", "Inquiry number, contact name, and phone are required.");

        var inquiry = new TradeInquiry
        {
            InquiryNumber = inquiryNumber.Trim(),
            UserId = userId,
            ContactName = contactName.Trim(),
            CompanyName = companyName?.Trim(),
            Email = email?.Trim(),
            PhoneNumber = phoneNumber.Trim(),
            InquiryType = type,
            Status = TradeInquiryStatus.New,
            Message = message?.Trim()
        };
        history.Add(TradeInquiryStatusHistory.Create(inquiry.Id, null, TradeInquiryStatus.New, null, userId, createdAt));
        return inquiry;
    }

    public void Assign(Guid assigneeId, Guid? actorId, DateTime at, ICollection<TradeInquiryStatusHistory> history)
    {
        if (assigneeId == Guid.Empty)
            throw new CommerceDomainException("TRADE_INQUIRY_ASSIGNEE_REQUIRED", "An assignee is required.");
        if (Status != TradeInquiryStatus.New)
            throw InvalidTransition(TradeInquiryStatus.Assigned);
        AssignedToUserId = assigneeId;
        TransitionTo(TradeInquiryStatus.Assigned, actorId, at, null, history);
    }

    public InquiryAttachment AttachMedia(ICollection<InquiryAttachment> attachments, Guid mediaAssetId,
        MediaVisibility visibility, bool mediaIsClean)
    {
        EnsureNotTerminal();
        if (!mediaIsClean)
            throw new CommerceDomainException("INQUIRY_ATTACHMENT_REQUIRES_CLEAN_MEDIA", "Only clean media can be attached to a trade inquiry.");
        if (attachments.Any(x => x.MediaAssetId == mediaAssetId))
            throw new CommerceDomainException("INQUIRY_ATTACHMENT_DUPLICATE", "The media asset is already attached to this inquiry.");
        var attachment = InquiryAttachment.CreateForTradeInquiry(Id, mediaAssetId, visibility);
        attachments.Add(attachment);
        return attachment;
    }

    public void RemoveAttachment(ICollection<InquiryAttachment> attachments, Guid mediaAssetId)
    {
        EnsureNotTerminal();
        var attachment = attachments.SingleOrDefault(x => x.MediaAssetId == mediaAssetId)
            ?? throw new CommerceDomainException("INQUIRY_ATTACHMENT_NOT_FOUND", "Inquiry attachment was not found.");
        attachments.Remove(attachment);
    }

    public void Unassign(string reason, Guid? actorId, DateTime at, ICollection<TradeInquiryStatusHistory> history)
    {
        EnsureReason(reason);
        if (Status != TradeInquiryStatus.Assigned)
            throw InvalidTransition(TradeInquiryStatus.New);
        AssignedToUserId = null;
        TransitionTo(TradeInquiryStatus.New, actorId, at, reason, history);
    }

    public void StartProgress(Guid? actorId, DateTime at, ICollection<TradeInquiryStatusHistory> history) =>
        TransitionFrom(TradeInquiryStatus.Assigned, TradeInquiryStatus.InProgress, actorId, at, history);

    public void MarkQuoted(Guid? actorId, DateTime at, ICollection<TradeInquiryStatusHistory> history) =>
        TransitionFrom(TradeInquiryStatus.InProgress, TradeInquiryStatus.Quoted, actorId, at, history);

    public void MarkWon(Guid? actorId, DateTime at, ICollection<TradeInquiryStatusHistory> history) =>
        TransitionFrom(TradeInquiryStatus.Quoted, TradeInquiryStatus.Won, actorId, at, history);

    public void MarkLost(string reason, Guid? actorId, DateTime at, ICollection<TradeInquiryStatusHistory> history)
    {
        EnsureReason(reason);
        if (Status != TradeInquiryStatus.Quoted)
            throw InvalidTransition(TradeInquiryStatus.Lost);
        TransitionTo(TradeInquiryStatus.Lost, actorId, at, reason, history);
    }

    public void Close(string reason, Guid? actorId, DateTime at, ICollection<TradeInquiryStatusHistory> history)
    {
        EnsureReason(reason);
        if (Status is TradeInquiryStatus.Won or TradeInquiryStatus.Lost or TradeInquiryStatus.Closed)
            throw InvalidTransition(TradeInquiryStatus.Closed);
        TransitionTo(TradeInquiryStatus.Closed, actorId, at, reason, history);
    }

    private void TransitionFrom(TradeInquiryStatus expected, TradeInquiryStatus target, Guid? actorId, DateTime at,
        ICollection<TradeInquiryStatusHistory> history)
    {
        if (Status != expected)
            throw InvalidTransition(target);
        TransitionTo(target, actorId, at, null, history);
    }

    private void TransitionTo(TradeInquiryStatus target, Guid? actorId, DateTime at, string? reason,
        ICollection<TradeInquiryStatusHistory> history)
    {
        var previous = Status;
        Status = target;
        history.Add(TradeInquiryStatusHistory.Create(Id, previous, target, reason, actorId, at));
        AddDomainEvent(new CommerceStateChangedEvent(nameof(TradeInquiry), Id, previous.ToString(), target.ToString()));
    }

    private CommerceDomainException InvalidTransition(TradeInquiryStatus target) =>
        new("TRADE_INQUIRY_STATUS_TRANSITION_INVALID", $"Trade inquiry cannot transition from {Status} to {target}.");

    private static void EnsureReason(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new CommerceDomainException("TRADE_INQUIRY_REASON_REQUIRED", "A reason is required.");
    }

    private void EnsureNotTerminal()
    {
        if (Status is TradeInquiryStatus.Won or TradeInquiryStatus.Lost or TradeInquiryStatus.Closed)
            throw new CommerceDomainException("TRADE_INQUIRY_TERMINAL", "A terminal trade inquiry cannot change attachments.");
    }

    private TradeInquiry()
    {
    }
}
