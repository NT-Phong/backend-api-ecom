namespace Ecom.Domain.Entities;

public class IdempotencyRecord : BaseEntity
{
    public string Operation { get; private set; } = string.Empty;
    public string OwnerScope { get; private set; } = string.Empty;
    public string KeyHash { get; private set; } = string.Empty;
    public string RequestFingerprint { get; private set; } = string.Empty;
    public IdempotencyStatus Status { get; private set; }
    public Guid? OrderId { get; private set; }
    public DateTime ExpiresAt { get; private set; }

    public static IdempotencyRecord Start(string operation, string ownerScope, string keyHash,
        string requestFingerprint, DateTime expiresAt)
    {
        if (string.IsNullOrWhiteSpace(operation) || string.IsNullOrWhiteSpace(ownerScope) ||
            string.IsNullOrWhiteSpace(keyHash) || string.IsNullOrWhiteSpace(requestFingerprint) || expiresAt == default)
            throw new CommerceDomainException("IDEMPOTENCY_RECORD_INVALID", "Idempotency data is required.");

        return new IdempotencyRecord
        {
            Operation = operation.Trim(), OwnerScope = ownerScope.Trim(), KeyHash = keyHash.Trim(),
            RequestFingerprint = requestFingerprint.Trim(), Status = IdempotencyStatus.Processing, ExpiresAt = expiresAt
        };
    }

    public void Complete(Guid orderId)
    {
        if (Status != IdempotencyStatus.Processing || orderId == Guid.Empty)
            throw new CommerceDomainException("IDEMPOTENCY_COMPLETE_INVALID", "The idempotency request cannot be completed.");
        Status = IdempotencyStatus.Completed;
        OrderId = orderId;
    }

    private IdempotencyRecord() { }
}
