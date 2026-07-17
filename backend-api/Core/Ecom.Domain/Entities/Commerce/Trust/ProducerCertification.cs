namespace Ecom.Domain.Entities;
public class ProducerCertification : BaseEntity
{
    public Guid ProducerId { get; private set; }
    public Guid CertificationId { get; private set; }

    private ProducerCertification()
    {
    }
}