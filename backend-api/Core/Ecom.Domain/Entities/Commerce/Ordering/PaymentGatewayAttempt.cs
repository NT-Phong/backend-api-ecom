namespace Ecom.Domain.Entities;

/// <summary>
/// Immutable local correlation record for an external payment-provider checkout.
/// It intentionally excludes raw provider payloads and payment-card data.
/// </summary>
public class PaymentGatewayAttempt : BaseEntity
{
    public Guid PaymentId { get; private set; }
    public string Provider { get; private set; } = string.Empty;
    public string InvoiceNumber { get; private set; } = string.Empty;
    public decimal ExpectedAmount { get; private set; }
    public string CurrencyCode { get; private set; } = string.Empty;
    public PaymentGatewayAttemptStatus Status { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? CheckoutIssuedAt { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public DateTime? LastNotificationAt { get; private set; }
    public string? ExternalOrderId { get; private set; }
    public string? ExternalTransactionId { get; private set; }
    public string? ExternalTransactionReference { get; private set; }
    public string? ProviderOrderStatus { get; private set; }
    public string? ProviderTransactionStatus { get; private set; }

    public static PaymentGatewayAttempt Create(Guid paymentId, string provider, string invoiceNumber,
        decimal expectedAmount, string currencyCode, DateTime expiresAt)
    {
        if (paymentId == Guid.Empty || string.IsNullOrWhiteSpace(provider) || string.IsNullOrWhiteSpace(invoiceNumber) ||
            string.IsNullOrWhiteSpace(currencyCode) || expectedAmount <= 0 || expiresAt == default)
            throw new CommerceDomainException("PAYMENT_GATEWAY_ATTEMPT_INVALID", "Payment gateway attempt details are invalid.");

        return new PaymentGatewayAttempt
        {
            PaymentId = paymentId,
            Provider = provider.Trim(),
            InvoiceNumber = invoiceNumber.Trim(),
            ExpectedAmount = expectedAmount,
            CurrencyCode = currencyCode.Trim().ToUpperInvariant(),
            ExpiresAt = expiresAt,
            Status = PaymentGatewayAttemptStatus.Created
        };
    }

    public void MarkCheckoutIssued(DateTime issuedAt)
    {
        if (Status is PaymentGatewayAttemptStatus.Paid or PaymentGatewayAttemptStatus.NeedsReconciliation)
            throw new CommerceDomainException("PAYMENT_GATEWAY_ATTEMPT_TERMINAL", "A terminal payment attempt cannot issue checkout.");
        if (issuedAt == default)
            throw new CommerceDomainException("PAYMENT_GATEWAY_ATTEMPT_INVALID", "Checkout issue time is required.");

        CheckoutIssuedAt ??= issuedAt;
        Status = PaymentGatewayAttemptStatus.CheckoutIssued;
    }

    public void MarkPaid(string externalOrderId, string externalTransactionId, string? externalTransactionReference,
        string providerOrderStatus, string providerTransactionStatus, DateTime paidAt, DateTime notificationAt)
    {
        if (Status == PaymentGatewayAttemptStatus.NeedsReconciliation ||
            string.IsNullOrWhiteSpace(externalOrderId) || string.IsNullOrWhiteSpace(externalTransactionId) ||
            string.IsNullOrWhiteSpace(providerOrderStatus) || string.IsNullOrWhiteSpace(providerTransactionStatus) ||
            paidAt == default || notificationAt == default)
            throw new CommerceDomainException("PAYMENT_GATEWAY_ATTEMPT_INVALID", "Payment gateway confirmation details are invalid.");

        Status = PaymentGatewayAttemptStatus.Paid;
        ExternalOrderId = externalOrderId.Trim();
        ExternalTransactionId = externalTransactionId.Trim();
        ExternalTransactionReference = string.IsNullOrWhiteSpace(externalTransactionReference) ? null : externalTransactionReference.Trim();
        ProviderOrderStatus = providerOrderStatus.Trim();
        ProviderTransactionStatus = providerTransactionStatus.Trim();
        PaidAt = paidAt;
        LastNotificationAt = notificationAt;
    }

    public void MarkNeedsReconciliation(DateTime notificationAt)
    {
        if (notificationAt == default)
            throw new CommerceDomainException("PAYMENT_GATEWAY_ATTEMPT_INVALID", "Notification time is required.");

        Status = PaymentGatewayAttemptStatus.NeedsReconciliation;
        LastNotificationAt = notificationAt;
    }

    private PaymentGatewayAttempt() { }
}
