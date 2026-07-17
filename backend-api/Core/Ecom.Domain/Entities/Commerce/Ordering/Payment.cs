namespace Ecom.Domain.Entities;
public class Payment : BaseEntity
{
    public Guid OrderId { get; private set; }
    public PaymentMethod Method { get; private set; }
    public PaymentStatus Status { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime? DueAt { get; private set; }
    public DateTime? PaidAt { get; private set; }

    private Payment()
    {
    }
}