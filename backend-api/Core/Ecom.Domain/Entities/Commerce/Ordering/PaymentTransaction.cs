namespace Ecom.Domain.Entities;
public class PaymentTransaction : BaseEntity
{
    public Guid PaymentId { get; private set; }
    public string Provider { get; private set; } = string.Empty;
    public string? ProviderReference { get; private set; }
    public PaymentTransactionType TransactionType { get; private set; }
    public PaymentStatus Status { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime OccurredAt { get; private set; }
    public Guid? ProofMediaAssetId { get; private set; }

    private PaymentTransaction()
    {
    }
}