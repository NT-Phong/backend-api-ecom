namespace Ecom.Domain.Entities;
public class FacilityCertification : BaseEntity
{
    public Guid ProductionFacilityId { get; private set; }
    public Guid CertificationId { get; private set; }

    private FacilityCertification()
    {
    }
}