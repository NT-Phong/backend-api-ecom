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

    internal static PaymentTransaction Create(Guid paymentId, string provider, string? providerReference,
        PaymentTransactionType type, PaymentStatus status, decimal amount, DateTime occurredAt, Guid? proofMediaAssetId = null)
    {
        if (paymentId == Guid.Empty || string.IsNullOrWhiteSpace(provider))
            throw new CommerceDomainException("PAYMENT_TRANSACTION_REFERENCE_REQUIRED", "Payment and provider are required.");
        if (amount < 0 || occurredAt == default)
            throw new CommerceDomainException("PAYMENT_TRANSACTION_INVALID", "Payment transaction amount or time is invalid.");

        return new PaymentTransaction
        {
            PaymentId = paymentId,
            Provider = provider.Trim(),
            ProviderReference = providerReference?.Trim(),
            TransactionType = type,
            Status = status,
            Amount = amount,
            OccurredAt = occurredAt,
            ProofMediaAssetId = proofMediaAssetId
        };
    }

    private PaymentTransaction()
    {
    }
}
