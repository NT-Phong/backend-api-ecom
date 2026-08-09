namespace Ecom.Domain.Entities;
public class Payment : BaseEntity, IAggregateRoot
{
    public Guid OrderId { get; private set; }
    public PaymentMethod Method { get; private set; }
    public PaymentStatus Status { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime? DueAt { get; private set; }
    public DateTime? PaidAt { get; private set; }

    public static Payment Create(Guid orderId, PaymentMethod method, decimal amount, DateTime? dueAt = null)
    {
        if (orderId == Guid.Empty)
            throw new CommerceDomainException("PAYMENT_ORDER_REQUIRED", "An order is required.");
        if (amount < 0)
            throw new CommerceDomainException("PAYMENT_AMOUNT_INVALID", "Payment amount cannot be negative.");
        if (method == PaymentMethod.Gateway)
            throw new CommerceDomainException("PAYMENT_METHOD_UNSUPPORTED", "Gateway payment is not supported in the MVP.");

        return new Payment
        {
            OrderId = orderId,
            Method = method,
            Amount = amount,
            DueAt = dueAt,
            Status = method == PaymentMethod.BankTransfer ? PaymentStatus.AwaitingConfirmation : PaymentStatus.Pending
        };
    }

    public bool RequiresPrepayment() => Method is PaymentMethod.BankTransfer or PaymentMethod.SePay or PaymentMethod.SePayVietQr;

    public PaymentTransaction MarkAwaitingConfirmation(string provider, DateTime occurredAt)
    {
        if (Status != PaymentStatus.Pending)
            throw InvalidTransition(PaymentStatus.AwaitingConfirmation);
        ChangeStatus(PaymentStatus.AwaitingConfirmation);
        return PaymentTransaction.Create(Id, provider, null, PaymentTransactionType.Initiate, Status, Amount, occurredAt);
    }

    public PaymentTransaction MarkPaid(decimal amount, string provider, string? reference, DateTime paidAt,
        Guid? proofMediaAssetId = null, bool proofIsCleanAndRestricted = false)
    {
        if (Status is not (PaymentStatus.Pending or PaymentStatus.AwaitingConfirmation))
            throw InvalidTransition(PaymentStatus.Paid);
        EnsureMatchingAmount(amount);
        if (proofMediaAssetId.HasValue && !proofIsCleanAndRestricted)
            throw new CommerceDomainException("PAYMENT_PROOF_INVALID", "Payment proof must be clean and restricted.");
        ChangeStatus(PaymentStatus.Paid);
        PaidAt = paidAt;
        return PaymentTransaction.Create(Id, provider, reference, PaymentTransactionType.Verify, Status, amount, paidAt, proofMediaAssetId);
    }

    public PaymentTransaction MarkFailed(string provider, string? reference, DateTime occurredAt)
    {
        if (Status is not (PaymentStatus.Pending or PaymentStatus.AwaitingConfirmation))
            throw InvalidTransition(PaymentStatus.Failed);
        ChangeStatus(PaymentStatus.Failed);
        return PaymentTransaction.Create(Id, provider, reference, PaymentTransactionType.Verify, Status, Amount, occurredAt);
    }

    public PaymentTransaction Cancel(string provider, DateTime occurredAt)
    {
        if (Status is not (PaymentStatus.Pending or PaymentStatus.AwaitingConfirmation))
            throw InvalidTransition(PaymentStatus.Cancelled);
        ChangeStatus(PaymentStatus.Cancelled);
        return PaymentTransaction.Create(Id, provider, null, PaymentTransactionType.Verify, Status, Amount, occurredAt);
    }

    public PaymentTransaction Refund(decimal amount, string provider, string? reference, DateTime occurredAt)
    {
        if (Status != PaymentStatus.Paid)
            throw InvalidTransition(PaymentStatus.Refunded);
        EnsureMatchingAmount(amount);
        ChangeStatus(PaymentStatus.Refunded);
        return PaymentTransaction.Create(Id, provider, reference, PaymentTransactionType.Refund, Status, amount, occurredAt);
    }

    private void EnsureMatchingAmount(decimal amount)
    {
        if (amount != Amount)
            throw new CommerceDomainException("PAYMENT_AMOUNT_MISMATCH", "Payment amount must match the order payment amount.");
    }

    private CommerceDomainException InvalidTransition(PaymentStatus target) =>
        new("PAYMENT_STATUS_TRANSITION_INVALID", $"Payment cannot transition from {Status} to {target}.");

    private void ChangeStatus(PaymentStatus target)
    {
        var previous = Status;
        Status = target;
        AddDomainEvent(new CommerceStateChangedEvent(nameof(Payment), Id, previous.ToString(), target.ToString()));
    }

    private Payment()
    {
    }
}
