namespace Ecom.Domain.Entities;
public class ProductCertification : BaseEntity
{
    public Guid ProductId { get; private set; }
    public Guid CertificationId { get; private set; }

    private ProductCertification()
    {
    }
}