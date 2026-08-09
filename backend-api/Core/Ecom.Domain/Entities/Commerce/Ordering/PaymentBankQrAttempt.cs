namespace Ecom.Domain.Entities;

/// <summary>Server-owned correlation for a SePay Bank Webhook VietQR payment.</summary>
public sealed class PaymentBankQrAttempt : BaseEntity
{
    public Guid PaymentId { get; private set; }
    public string Provider { get; private set; } = string.Empty;
    public string PaymentCode { get; private set; } = string.Empty;
    public decimal ExpectedAmount { get; private set; }
    public string CurrencyCode { get; private set; } = string.Empty;
    public string VirtualAccountFingerprint { get; private set; } = string.Empty;
    public PaymentBankQrAttemptStatus Status { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? QrIssuedAt { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public DateTime? LastNotificationAt { get; private set; }
    public string? ExternalTransactionId { get; private set; }
    public string? ExternalTransactionReference { get; private set; }

    public static PaymentBankQrAttempt Create(Guid paymentId, string provider, string paymentCode, decimal expectedAmount,
        string currencyCode, string virtualAccountFingerprint, DateTime expiresAt)
    {
        if (paymentId == Guid.Empty || string.IsNullOrWhiteSpace(provider) || string.IsNullOrWhiteSpace(paymentCode) ||
            expectedAmount <= 0 || string.IsNullOrWhiteSpace(currencyCode) || string.IsNullOrWhiteSpace(virtualAccountFingerprint) || expiresAt == default)
            throw new CommerceDomainException("PAYMENT_BANK_QR_ATTEMPT_INVALID", "Bank QR payment attempt details are invalid.");

        return new PaymentBankQrAttempt
        {
            PaymentId = paymentId, Provider = provider.Trim(), PaymentCode = paymentCode.Trim().ToUpperInvariant(),
            ExpectedAmount = expectedAmount, CurrencyCode = currencyCode.Trim().ToUpperInvariant(),
            VirtualAccountFingerprint = virtualAccountFingerprint.Trim(), ExpiresAt = expiresAt,
            Status = PaymentBankQrAttemptStatus.Created
        };
    }

    public void MarkQrIssued(DateTime issuedAt)
    {
        if (Status is PaymentBankQrAttemptStatus.Paid or PaymentBankQrAttemptStatus.NeedsReconciliation || issuedAt == default)
            throw new CommerceDomainException("PAYMENT_BANK_QR_ATTEMPT_INVALID", "A terminal bank QR attempt cannot issue a QR.");
        QrIssuedAt ??= issuedAt;
        Status = PaymentBankQrAttemptStatus.QrIssued;
    }

    public void MarkPaid(string externalTransactionId, string? externalTransactionReference, DateTime paidAt, DateTime notificationAt)
    {
        if (Status == PaymentBankQrAttemptStatus.NeedsReconciliation || string.IsNullOrWhiteSpace(externalTransactionId) ||
            paidAt == default || notificationAt == default)
            throw new CommerceDomainException("PAYMENT_BANK_QR_ATTEMPT_INVALID", "Bank QR confirmation details are invalid.");
        Status = PaymentBankQrAttemptStatus.Paid;
        ExternalTransactionId = externalTransactionId.Trim();
        ExternalTransactionReference = string.IsNullOrWhiteSpace(externalTransactionReference) ? null : externalTransactionReference.Trim();
        PaidAt = paidAt; LastNotificationAt = notificationAt;
    }

    public void MarkNeedsReconciliation(DateTime notificationAt)
    {
        if (notificationAt == default) throw new CommerceDomainException("PAYMENT_BANK_QR_ATTEMPT_INVALID", "Notification time is required.");
        Status = PaymentBankQrAttemptStatus.NeedsReconciliation; LastNotificationAt = notificationAt;
    }

    private PaymentBankQrAttempt() { }
}
