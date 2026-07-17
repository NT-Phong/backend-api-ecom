namespace Ecom.Domain.Entities;
public class CertificationEvidence : BaseEntity
{
    public Guid CertificationId { get; private set; }
    public Guid MediaAssetId { get; private set; }
    public CertificationEvidenceType EvidenceType { get; private set; }

    private CertificationEvidence()
    {
    }
}