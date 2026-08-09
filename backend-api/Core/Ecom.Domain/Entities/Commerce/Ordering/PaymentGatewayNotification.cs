namespace Ecom.Domain.Entities;

/// <summary>
/// Immutable, normalized record of an authenticated provider notification.
/// Raw provider payloads and payment-card/customer data are deliberately excluded.
/// </summary>
public sealed class PaymentGatewayNotification : BaseEntity
{
    public Guid? PaymentGatewayAttemptId { get; private set; }
    public string Provider { get; private set; } = string.Empty;
    public string NotificationType { get; private set; } = string.Empty;
    public PaymentGatewayNotificationDisposition Disposition { get; private set; }
    public string? InvoiceNumber { get; private set; }
    public decimal? OrderAmount { get; private set; }
    public decimal? TransactionAmount { get; private set; }
    public string? CurrencyCode { get; private set; }
    public string? ExternalOrderId { get; private set; }
    public string? ExternalTransactionId { get; private set; }
    public string? ExternalTransactionReference { get; private set; }
    public string? ProviderOrderStatus { get; private set; }
    public string? ProviderTransactionStatus { get; private set; }
    public string? FailureReasonCode { get; private set; }
    public DateTime ReceivedAt { get; private set; }
    public DateTime? OccurredAt { get; private set; }

    public static PaymentGatewayNotification Create(Guid? attemptId, string provider, string notificationType,
        PaymentGatewayNotificationDisposition disposition, string? invoiceNumber, decimal? orderAmount,
        decimal? transactionAmount, string? currencyCode, string? externalOrderId, string? externalTransactionId,
        string? externalTransactionReference, string? providerOrderStatus, string? providerTransactionStatus,
        string? failureReasonCode, DateTime receivedAt, DateTime? occurredAt)
    {
        if (string.IsNullOrWhiteSpace(provider) || string.IsNullOrWhiteSpace(notificationType) || receivedAt == default)
            throw new CommerceDomainException("PAYMENT_GATEWAY_NOTIFICATION_INVALID", "Payment gateway notification details are invalid.");

        return new PaymentGatewayNotification
        {
            PaymentGatewayAttemptId = attemptId,
            Provider = provider.Trim(),
            NotificationType = notificationType.Trim(),
            Disposition = disposition,
            InvoiceNumber = Normalize(invoiceNumber),
            OrderAmount = orderAmount,
            TransactionAmount = transactionAmount,
            CurrencyCode = Normalize(currencyCode)?.ToUpperInvariant(),
            ExternalOrderId = Normalize(externalOrderId),
            ExternalTransactionId = Normalize(externalTransactionId),
            ExternalTransactionReference = Normalize(externalTransactionReference),
            ProviderOrderStatus = Normalize(providerOrderStatus),
            ProviderTransactionStatus = Normalize(providerTransactionStatus),
            FailureReasonCode = Normalize(failureReasonCode),
            ReceivedAt = receivedAt,
            OccurredAt = occurredAt
        };
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private PaymentGatewayNotification() { }
}
