using System.Globalization;
using System.Text.Json.Serialization;
using Ecom.Application.Common.Interfaces;
using Ecom.Domain.Entities;
using Ecom.Domain.Interfaces.Repositories;

namespace Ecom.Application.Features.Commerce.Payments.Commands.ProcessSePayIpn;

public sealed record SePayIpnOrder(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("order_status")] string? Status,
    [property: JsonPropertyName("order_currency")] string? Currency,
    [property: JsonPropertyName("order_amount")] string? Amount,
    [property: JsonPropertyName("order_invoice_number")] string? InvoiceNumber);

public sealed record SePayIpnTransaction(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("transaction_id")] string? Reference,
    [property: JsonPropertyName("transaction_status")] string? Status,
    [property: JsonPropertyName("transaction_currency")] string? Currency,
    [property: JsonPropertyName("transaction_amount")] string? Amount,
    [property: JsonPropertyName("transaction_date")] string? OccurredAt);

public sealed record SePayIpnPayload(
    [property: JsonPropertyName("timestamp")] long Timestamp,
    [property: JsonPropertyName("notification_type")] string? NotificationType,
    [property: JsonPropertyName("order")] SePayIpnOrder? Order,
    [property: JsonPropertyName("transaction")] SePayIpnTransaction? Transaction);

public sealed record ProcessSePayIpnCommand(string? Secret, SePayIpnPayload Payload) : IRequest<TResult>, ITransactionalRequest;

public sealed class ProcessSePayIpnCommandHandler(
    IUnitOfWork unitOfWork,
    IOrderLifecycleStore orderLifecycleStore,
    ISePayCheckoutService sePayCheckoutService)
    : IRequestHandler<ProcessSePayIpnCommand, TResult>
{
    private const string Provider = "sepay";

    public async Task<TResult> Handle(ProcessSePayIpnCommand request, CancellationToken cancellationToken)
    {
        if (!sePayCheckoutService.IsValidIpnSecret(request.Secret))
            return TResult.Failure(MessageKey.Unauthorized, ErrorCodes.UNAUTHORIZED);

        if (!TryValidatePayload(request.Payload, out var data))
            return TResult.Failure("Invalid SePay IPN payload.", ErrorCodes.UNPROCESSABLE_ENTITY);

        var now = DateTime.UtcNow;
        var notifications = unitOfWork.Repository<PaymentGatewayNotification>();
        var attempts = unitOfWork.Repository<PaymentGatewayAttempt>();
        var attempt = await orderLifecycleStore.LockPaymentGatewayAttemptAsync(Provider, data.InvoiceNumber, cancellationToken);
        if (attempt is null)
        {
            if (await IsDuplicateNotificationAsync(notifications, data, cancellationToken))
                return TResult.Success();
            await InsertNotificationAsync(notifications, null, data, PaymentGatewayNotificationDisposition.NeedsReconciliation,
                "ATTEMPT_NOT_FOUND", now, cancellationToken);
            return TResult.Success();
        }

        // The attempt lock serializes all notifications for the same invoice. Check for a duplicate only after
        // acquiring it so a concurrent retry observes the first committed notification.
        if (await IsDuplicateNotificationAsync(notifications, data, cancellationToken))
            return TResult.Success();

        if (!MatchesAttempt(attempt, data))
        {
            await RecordReconciliationAsync(attempts, notifications, attempt, data, "PAYMENT_MISMATCH", now, cancellationToken);
            return TResult.Success();
        }

        if (string.Equals(data.NotificationType, "TRANSACTION_VOID", StringComparison.Ordinal))
        {
            await RecordReconciliationAsync(attempts, notifications, attempt, data, "TRANSACTION_VOID", now, cancellationToken);
            return TResult.Success();
        }

        if (!IsApprovedPayment(data))
        {
            await RecordReconciliationAsync(attempts, notifications, attempt, data, "PAYMENT_STATUS_INVALID", now, cancellationToken);
            return TResult.Success();
        }

        var paymentSnapshot = await unitOfWork.Repository<Payment>().QueryNoTracking()
            .SingleOrDefaultAsync(x => x.Id == attempt.PaymentId, cancellationToken);
        if (paymentSnapshot is null)
        {
            await RecordReconciliationAsync(attempts, notifications, attempt, data, "PAYMENT_NOT_FOUND", now, cancellationToken);
            return TResult.Success();
        }

        // Keep the order -> payment locking order used by cancellation and checkout to avoid lock inversion.
        var order = await orderLifecycleStore.LockOrderAsync(paymentSnapshot.OrderId, cancellationToken);
        if (order is null)
        {
            await RecordReconciliationAsync(attempts, notifications, attempt, data, "ORDER_NOT_FOUND", now, cancellationToken);
            return TResult.Success();
        }
        var payment = await orderLifecycleStore.LockPaymentAsync(order.Id, cancellationToken);
        if (payment is null || payment.Method != PaymentMethod.SePay)
        {
            await RecordReconciliationAsync(attempts, notifications, attempt, data, "PAYMENT_NOT_SEPAY", now, cancellationToken);
            return TResult.Success();
        }

        var transactions = unitOfWork.Repository<PaymentTransaction>();
        var existingTransaction = await transactions.Query().FirstOrDefaultAsync(x => x.Provider == Provider &&
            x.ProviderReference == data.ExternalTransactionId, cancellationToken);
        if (existingTransaction is not null)
        {
            if (existingTransaction.PaymentId != payment.Id)
            {
                attempt.MarkNeedsReconciliation(now);
                await attempts.UpdateAsync(attempt, cancellationToken);
                await InsertNotificationAsync(notifications, attempt.Id, data, PaymentGatewayNotificationDisposition.NeedsReconciliation,
                    "TRANSACTION_ASSOCIATED_WITH_OTHER_PAYMENT", now, cancellationToken);
                return TResult.Success();
            }

            await InsertNotificationAsync(notifications, attempt.Id, data, PaymentGatewayNotificationDisposition.Duplicate,
                null, now, cancellationToken);
            return TResult.Success();
        }

        if (payment.Status != PaymentStatus.Pending || order.Status != OrderStatus.Pending ||
            attempt.Status == PaymentGatewayAttemptStatus.NeedsReconciliation)
        {
            await RecordReconciliationAsync(attempts, notifications, attempt, data, "LOCAL_PAYMENT_OR_ORDER_NOT_PENDING", now, cancellationToken);
            return TResult.Success();
        }

        // SePay documents transaction_date without an explicit timezone. Until that contract is clarified, this V1
        // uses the local receipt time as the expiry boundary: a callback received after the local deadline requires
        // staff reconciliation instead of silently accepting a potentially late payment.
        if (attempt.ExpiresAt <= now || payment.DueAt is null || payment.DueAt <= now)
        {
            await RecordReconciliationAsync(attempts, notifications, attempt, data, "LATE_PAYMENT", now, cancellationToken);
            return TResult.Success();
        }

        var paymentTransaction = payment.MarkPaid(payment.Amount, Provider, data.ExternalTransactionId, data.OccurredAt);
        attempt.MarkPaid(data.ExternalOrderId, data.ExternalTransactionId, data.ExternalTransactionReference,
            data.ProviderOrderStatus, data.ProviderTransactionStatus, data.OccurredAt, now);
        await unitOfWork.Repository<Payment>().UpdateAsync(payment, cancellationToken);
        await transactions.InsertAsync(paymentTransaction, cancellationToken);
        await attempts.UpdateAsync(attempt, cancellationToken);
        await InsertNotificationAsync(notifications, attempt.Id, data, PaymentGatewayNotificationDisposition.Accepted,
            null, now, cancellationToken);
        return TResult.Success();
    }

    private static Task<bool> IsDuplicateNotificationAsync(IBaseRepository<PaymentGatewayNotification> notifications,
        ValidatedIpn data, CancellationToken cancellationToken) => notifications.Query().AnyAsync(x => x.Provider == Provider &&
            x.NotificationType == data.NotificationType && x.ExternalTransactionId == data.ExternalTransactionId, cancellationToken);

    private static async Task RecordReconciliationAsync(IBaseRepository<PaymentGatewayAttempt> attempts,
        IBaseRepository<PaymentGatewayNotification> notifications, PaymentGatewayAttempt attempt, ValidatedIpn data,
        string failureReasonCode, DateTime receivedAt, CancellationToken cancellationToken)
    {
        attempt.MarkNeedsReconciliation(receivedAt);
        await attempts.UpdateAsync(attempt, cancellationToken);
        await InsertNotificationAsync(notifications, attempt.Id, data, PaymentGatewayNotificationDisposition.NeedsReconciliation,
            failureReasonCode, receivedAt, cancellationToken);
    }

    private static async Task InsertNotificationAsync(IBaseRepository<PaymentGatewayNotification> notifications, Guid? attemptId,
        ValidatedIpn data, PaymentGatewayNotificationDisposition disposition, string? failureReasonCode, DateTime receivedAt,
        CancellationToken cancellationToken) => await notifications.InsertAsync(PaymentGatewayNotification.Create(attemptId,
            Provider, data.NotificationType, disposition, data.InvoiceNumber, data.OrderAmount, data.TransactionAmount,
            data.OrderCurrency, data.ExternalOrderId, data.ExternalTransactionId, data.ExternalTransactionReference,
            data.ProviderOrderStatus, data.ProviderTransactionStatus, failureReasonCode, receivedAt, data.OccurredAt), cancellationToken);

    private static bool MatchesAttempt(PaymentGatewayAttempt attempt, ValidatedIpn data) =>
        attempt.ExpectedAmount == data.OrderAmount && attempt.ExpectedAmount == data.TransactionAmount &&
        string.Equals(attempt.CurrencyCode, "VND", StringComparison.OrdinalIgnoreCase) &&
        string.Equals(data.OrderCurrency, "VND", StringComparison.OrdinalIgnoreCase) &&
        string.Equals(data.TransactionCurrency, "VND", StringComparison.OrdinalIgnoreCase);

    private static bool IsApprovedPayment(ValidatedIpn data) =>
        string.Equals(data.NotificationType, "ORDER_PAID", StringComparison.Ordinal) &&
        string.Equals(data.ProviderOrderStatus, "CAPTURED", StringComparison.Ordinal) &&
        string.Equals(data.ProviderTransactionStatus, "APPROVED", StringComparison.Ordinal);

    private static bool TryValidatePayload(SePayIpnPayload payload, out ValidatedIpn data)
    {
        data = default!;
        var order = payload.Order;
        var transaction = payload.Transaction;
        if (order is null || transaction is null ||
            !string.Equals(payload.NotificationType, "ORDER_PAID", StringComparison.Ordinal) &&
            !string.Equals(payload.NotificationType, "TRANSACTION_VOID", StringComparison.Ordinal) ||
            string.IsNullOrWhiteSpace(order.Id) || string.IsNullOrWhiteSpace(order.InvoiceNumber) ||
            string.IsNullOrWhiteSpace(transaction.Id) || string.IsNullOrWhiteSpace(order.Currency) ||
            string.IsNullOrWhiteSpace(transaction.Currency) || string.IsNullOrWhiteSpace(order.Status) ||
            string.IsNullOrWhiteSpace(transaction.Status) ||
            !decimal.TryParse(order.Amount, NumberStyles.Number, CultureInfo.InvariantCulture, out var orderAmount) ||
            !decimal.TryParse(transaction.Amount, NumberStyles.Number, CultureInfo.InvariantCulture, out var transactionAmount) ||
            orderAmount <= 0 || transactionAmount <= 0 ||
            !DateTime.TryParse(transaction.OccurredAt, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var occurredAt))
            return false;

        data = new ValidatedIpn(payload.NotificationType!, order.Id, order.InvoiceNumber, order.Status, order.Currency,
            orderAmount, transaction.Id, transaction.Reference, transaction.Status, transaction.Currency, transactionAmount, occurredAt);
        return true;
    }

    private sealed record ValidatedIpn(string NotificationType, string ExternalOrderId, string InvoiceNumber,
        string ProviderOrderStatus, string OrderCurrency, decimal OrderAmount, string ExternalTransactionId,
        string? ExternalTransactionReference, string ProviderTransactionStatus, string TransactionCurrency,
        decimal TransactionAmount, DateTime OccurredAt);
}
