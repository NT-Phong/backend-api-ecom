using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ecom.Application.Common.Interfaces;
using Ecom.Domain.Entities;
using Ecom.Domain.Interfaces.Repositories;

namespace Ecom.Application.Features.Commerce.Payments.Commands.ProcessSePayBankWebhook;

/// <summary>SePay Bank Webhook payload. It is intentionally separate from the Hosted Checkout IPN contract.</summary>
public sealed record SePayBankWebhookPayload(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("gateway")] string? Gateway,
    [property: JsonPropertyName("transactionDate")] string? TransactionDate,
    [property: JsonPropertyName("accountNumber")] string? AccountNumber,
    [property: JsonPropertyName("code")] string? Code,
    [property: JsonPropertyName("content")] string? Content,
    [property: JsonPropertyName("transferType")] string? TransferType,
    [property: JsonPropertyName("transferAmount")] decimal TransferAmount,
    [property: JsonPropertyName("referenceCode")] string? ReferenceCode);

public sealed record ProcessSePayBankWebhookCommand(string? Timestamp, string RawBody, string? Signature)
    : IRequest<TResult>, ITransactionalRequest;

public sealed class ProcessSePayBankWebhookCommandHandler(
    IUnitOfWork unitOfWork,
    IOrderLifecycleStore orderLifecycleStore,
    ISePayBankQrService sePayBankQrService)
    : IRequestHandler<ProcessSePayBankWebhookCommand, TResult>
{
    private const string Provider = "sepay-bank-qr";
    private const string NotificationType = "BANK_TRANSFER_IN";
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    public async Task<TResult> Handle(ProcessSePayBankWebhookCommand request, CancellationToken cancellationToken)
    {
        if (!sePayBankQrService.IsValidWebhookSignature(request.Timestamp, request.RawBody, request.Signature))
            return TResult.Failure(MessageKey.Unauthorized, ErrorCodes.UNAUTHORIZED);

        SePayBankWebhookPayload? payload;
        try { payload = JsonSerializer.Deserialize<SePayBankWebhookPayload>(request.RawBody, SerializerOptions); }
        catch (JsonException) { return TResult.Failure("Invalid SePay Bank Webhook payload.", ErrorCodes.UNPROCESSABLE_ENTITY); }
        if (!TryValidatePayload(payload, out var data))
            return TResult.Failure("Invalid SePay Bank Webhook payload.", ErrorCodes.UNPROCESSABLE_ENTITY);

        var now = DateTime.UtcNow;
        var notifications = unitOfWork.Repository<PaymentBankQrWebhookNotification>();
        var attempts = unitOfWork.Repository<PaymentBankQrAttempt>();
        var attempt = await orderLifecycleStore.LockPaymentBankQrAttemptAsync(Provider, data.PaymentCode, cancellationToken);
        if (attempt is null)
        {
            if (await IsDuplicateNotificationAsync(notifications, data, cancellationToken)) return TResult.Success();
            await InsertNotificationAsync(notifications, null, data, PaymentBankQrNotificationDisposition.NeedsReconciliation,
                "ATTEMPT_NOT_FOUND", now, cancellationToken);
            return TResult.Success();
        }

        // This row lock makes valid retries for one payment code idempotent.
        if (await IsDuplicateNotificationAsync(notifications, data, cancellationToken)) return TResult.Success();
        if (!sePayBankQrService.IsExpectedVirtualAccount(data.AccountNumber))
        {
            await RecordReconciliationAsync(attempts, notifications, attempt, data, "VIRTUAL_ACCOUNT_MISMATCH", now, cancellationToken);
            return TResult.Success();
        }
        if (!MatchesAttempt(attempt, data))
        {
            await RecordReconciliationAsync(attempts, notifications, attempt, data, "PAYMENT_MISMATCH", now, cancellationToken);
            return TResult.Success();
        }

        var paymentSnapshot = await unitOfWork.Repository<Payment>().QueryNoTracking()
            .SingleOrDefaultAsync(x => x.Id == attempt.PaymentId, cancellationToken);
        if (paymentSnapshot is null)
        {
            await RecordReconciliationAsync(attempts, notifications, attempt, data, "PAYMENT_NOT_FOUND", now, cancellationToken);
            return TResult.Success();
        }

        // Keep the order -> payment order consistent with checkout/cancellation and Hosted Checkout IPN.
        var order = await orderLifecycleStore.LockOrderAsync(paymentSnapshot.OrderId, cancellationToken);
        if (order is null)
        {
            await RecordReconciliationAsync(attempts, notifications, attempt, data, "ORDER_NOT_FOUND", now, cancellationToken);
            return TResult.Success();
        }
        var payment = await orderLifecycleStore.LockPaymentAsync(order.Id, cancellationToken);
        if (payment is null || payment.Method != PaymentMethod.SePayVietQr)
        {
            await RecordReconciliationAsync(attempts, notifications, attempt, data, "PAYMENT_NOT_SEPAY_VIETQR", now, cancellationToken);
            return TResult.Success();
        }

        var transactions = unitOfWork.Repository<PaymentTransaction>();
        var existingTransaction = await transactions.Query().FirstOrDefaultAsync(x => x.Provider == Provider &&
            x.ProviderReference == data.ExternalTransactionId, cancellationToken);
        if (existingTransaction is not null)
        {
            if (existingTransaction.PaymentId != payment.Id)
            {
                await RecordReconciliationAsync(attempts, notifications, attempt, data,
                    "TRANSACTION_ASSOCIATED_WITH_OTHER_PAYMENT", now, cancellationToken);
                return TResult.Success();
            }
            await InsertNotificationAsync(notifications, attempt.Id, data, PaymentBankQrNotificationDisposition.Duplicate,
                null, now, cancellationToken);
            return TResult.Success();
        }

        if (payment.Status != PaymentStatus.Pending || order.Status != OrderStatus.Pending ||
            attempt.Status == PaymentBankQrAttemptStatus.NeedsReconciliation)
        {
            await RecordReconciliationAsync(attempts, notifications, attempt, data, "LOCAL_PAYMENT_OR_ORDER_NOT_PENDING", now, cancellationToken);
            return TResult.Success();
        }
        if (attempt.ExpiresAt <= now || payment.DueAt is null || payment.DueAt <= now)
        {
            await RecordReconciliationAsync(attempts, notifications, attempt, data, "LATE_PAYMENT", now, cancellationToken);
            return TResult.Success();
        }

        var transaction = payment.MarkPaid(payment.Amount, Provider, data.ExternalTransactionId, data.OccurredAt);
        attempt.MarkPaid(data.ExternalTransactionId, data.ExternalTransactionReference, data.OccurredAt, now);
        await unitOfWork.Repository<Payment>().UpdateAsync(payment, cancellationToken);
        await transactions.InsertAsync(transaction, cancellationToken);
        await attempts.UpdateAsync(attempt, cancellationToken);
        await InsertNotificationAsync(notifications, attempt.Id, data, PaymentBankQrNotificationDisposition.Accepted,
            null, now, cancellationToken);
        return TResult.Success();
    }

    private static Task<bool> IsDuplicateNotificationAsync(IBaseRepository<PaymentBankQrWebhookNotification> notifications,
        ValidatedBankWebhook data, CancellationToken cancellationToken) => notifications.Query().AnyAsync(x =>
            x.Provider == Provider && x.NotificationType == NotificationType && x.ExternalTransactionId == data.ExternalTransactionId,
            cancellationToken);

    private static async Task RecordReconciliationAsync(IBaseRepository<PaymentBankQrAttempt> attempts,
        IBaseRepository<PaymentBankQrWebhookNotification> notifications, PaymentBankQrAttempt attempt,
        ValidatedBankWebhook data, string failureReasonCode, DateTime receivedAt, CancellationToken cancellationToken)
    {
        attempt.MarkNeedsReconciliation(receivedAt);
        await attempts.UpdateAsync(attempt, cancellationToken);
        await InsertNotificationAsync(notifications, attempt.Id, data, PaymentBankQrNotificationDisposition.NeedsReconciliation,
            failureReasonCode, receivedAt, cancellationToken);
    }

    private static Task InsertNotificationAsync(IBaseRepository<PaymentBankQrWebhookNotification> notifications,
        Guid? attemptId, ValidatedBankWebhook data, PaymentBankQrNotificationDisposition disposition,
        string? failureReasonCode, DateTime receivedAt, CancellationToken cancellationToken) => notifications.InsertAsync(
            PaymentBankQrWebhookNotification.Create(attemptId, Provider, NotificationType, disposition, data.PaymentCode,
                data.TransferAmount, "VND", data.ExternalTransactionId, data.ExternalTransactionReference,
                failureReasonCode, receivedAt, data.OccurredAt), cancellationToken);

    private static bool MatchesAttempt(PaymentBankQrAttempt attempt, ValidatedBankWebhook data) =>
        attempt.ExpectedAmount == data.TransferAmount &&
        string.Equals(attempt.CurrencyCode, "VND", StringComparison.OrdinalIgnoreCase);

    private static bool TryValidatePayload(SePayBankWebhookPayload? payload, out ValidatedBankWebhook data)
    {
        data = default!;
        if (payload is null || payload.Id <= 0 || string.IsNullOrWhiteSpace(payload.AccountNumber) ||
            string.IsNullOrWhiteSpace(payload.Code) || string.IsNullOrWhiteSpace(payload.TransferType) ||
            !string.Equals(payload.TransferType, "in", StringComparison.OrdinalIgnoreCase) || payload.TransferAmount <= 0 ||
            !DateTime.TryParse(payload.TransactionDate, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var occurredAt)) return false;

        data = new ValidatedBankWebhook(payload.AccountNumber, payload.Code.Trim().ToUpperInvariant(),
            payload.TransferAmount, payload.Id.ToString(CultureInfo.InvariantCulture), payload.ReferenceCode, occurredAt);
        return true;
    }

    private sealed record ValidatedBankWebhook(string AccountNumber, string PaymentCode, decimal TransferAmount,
        string ExternalTransactionId, string? ExternalTransactionReference, DateTime OccurredAt);
}
