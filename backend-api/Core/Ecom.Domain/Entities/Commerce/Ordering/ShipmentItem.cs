namespace Ecom.Domain.Entities;
public class ShipmentItem : BaseEntity
{
    public Guid ShipmentId { get; private set; }
    public Guid OrderItemId { get; private set; }
    public int Quantity { get; private set; }

    private ShipmentItem()
    {
    }
}