namespace Ecom.Domain.Entities;
public class PointOfSaleProduct : BaseEntity
{
    public Guid PointOfSaleId { get; private set; }
    public Guid ProductId { get; private set; }
    public bool IsAvailable { get; private set; } = true;

    private PointOfSaleProduct()
    {
    }
}