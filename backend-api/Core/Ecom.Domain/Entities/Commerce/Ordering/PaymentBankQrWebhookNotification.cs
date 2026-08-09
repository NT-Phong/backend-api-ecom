namespace Ecom.Domain.Entities;

/// <summary>Normalized Bank Webhook audit record; raw payload and account data are deliberately excluded.</summary>
public sealed class PaymentBankQrWebhookNotification : BaseEntity
{
    public Guid? PaymentBankQrAttemptId { get; private set; }
    public string Provider { get; private set; } = string.Empty;
    public string NotificationType { get; private set; } = string.Empty;
    public PaymentBankQrNotificationDisposition Disposition { get; private set; }
    public string? PaymentCode { get; private set; }
    public decimal? TransactionAmount { get; private set; }
    public string? CurrencyCode { get; private set; }
    public string? ExternalTransactionId { get; private set; }
    public string? ExternalTransactionReference { get; private set; }
    public string? FailureReasonCode { get; private set; }
    public DateTime ReceivedAt { get; private set; }
    public DateTime? OccurredAt { get; private set; }

    public static PaymentBankQrWebhookNotification Create(Guid? attemptId, string provider, string notificationType,
        PaymentBankQrNotificationDisposition disposition, string? paymentCode, decimal? transactionAmount, string? currencyCode,
        string? externalTransactionId, string? externalTransactionReference, string? failureReasonCode, DateTime receivedAt, DateTime? occurredAt) =>
        new()
        {
            PaymentBankQrAttemptId = attemptId, Provider = provider.Trim(), NotificationType = notificationType.Trim(), Disposition = disposition,
            PaymentCode = Normalize(paymentCode), TransactionAmount = transactionAmount, CurrencyCode = Normalize(currencyCode)?.ToUpperInvariant(),
            ExternalTransactionId = Normalize(externalTransactionId), ExternalTransactionReference = Normalize(externalTransactionReference),
            FailureReasonCode = Normalize(failureReasonCode), ReceivedAt = receivedAt, OccurredAt = occurredAt
        };

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private PaymentBankQrWebhookNotification() { }
}
