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

    private Producer()
    {
    }
}