namespace Ecom.Domain.Entities;
public class Producer : BaseEntity
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? LegalName { get; private set; }
    public string? Description { get; private set; }
    public string? WebsiteUrl { get; private set; }
    public PublicStatus PublicStatus { get; private set; }
    public bool IsVerified { get; private set; }
    public DateTime? VerifiedAt { get; private set; }
    public Guid? VerifiedByUserId { get; private set; }

    public static Producer Create(string code, string name, string? legalName, string? description, string? websiteUrl)
    {
        var producer = new Producer { PublicStatus = PublicStatus.Draft };
        producer.UpdateDetails(code, name, legalName, description, websiteUrl);
        return producer;
    }

    public void UpdateDetails(string code, string name, string? legalName, string? description, string? websiteUrl)
    {
        if (PublicStatus == PublicStatus.Hidden)
            throw new CommerceDomainException("PRODUCER_HIDDEN", "A hidden producer cannot be updated.");
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
            throw new CommerceDomainException("PRODUCER_DETAILS_REQUIRED", "Producer code and name are required.");
        Code = code.Trim();
        Name = name.Trim();
        LegalName = legalName?.Trim();
        Description = description?.Trim();
        WebsiteUrl = websiteUrl?.Trim();
    }

    public void Verify(Guid actorUserId, DateTime verifiedAt)
    {
        if (PublicStatus == PublicStatus.Hidden)
            throw new CommerceDomainException("PRODUCER_HIDDEN", "A hidden producer cannot be verified.");
        if (actorUserId == Guid.Empty || verifiedAt == default)
            throw new CommerceDomainException("PRODUCER_VERIFY_DETAILS_REQUIRED", "A verifier and verification time are required.");
        IsVerified = true;
        VerifiedAt = verifiedAt;
        VerifiedByUserId = actorUserId;
    }

    public void Publish()
    {
        if (PublicStatus == PublicStatus.Hidden)
            throw new CommerceDomainException("PRODUCER_HIDDEN", "A hidden producer cannot be published.");
        if (!IsVerified)
            throw new CommerceDomainException("PRODUCER_VERIFICATION_REQUIRED", "Producer verification is required before publication.");
        PublicStatus = PublicStatus.Published;
    }

    public void Hide()
    {
        if (PublicStatus == PublicStatus.Hidden)
            throw new CommerceDomainException("PRODUCER_HIDDEN", "The producer is already hidden.");
        PublicStatus = PublicStatus.Hidden;
    }

    public Guid RenewConcurrencyStamp()
    {
        ConcurrencyStamp = Guid.NewGuid();
        return ConcurrencyStamp;
    }

    private Producer()
    {
    }
}
