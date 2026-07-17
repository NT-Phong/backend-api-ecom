namespace Ecom.Domain.Entities;
public class Certification : BaseEntity
{
    public string CertificationType { get; private set; } = string.Empty;
    public string CertificateNumber { get; private set; } = string.Empty;
    public string IssuingAuthority { get; private set; } = string.Empty;
    public DateOnly? IssuedAt { get; private set; }
    public DateOnly? EffectiveFrom { get; private set; }
    public DateOnly? EffectiveTo { get; private set; }
    public CertificationVerificationStatus VerificationStatus { get; private set; }
    public DateTime? VerifiedAt { get; private set; }
    public Guid? VerifiedByUserId { get; private set; }

    private Certification()
    {
    }
}