using System.Globalization;
using Ecom.Application.Common.Configuration;
using Ecom.Application.Common.Interfaces;
using Ecom.Application.Common.Models;
using Ecom.Application.Features.Commerce.Payments.Commands.ProcessSePayIpn;
using Ecom.Domain.Entities;
using Ecom.Domain.Enums;
using Ecom.Domain.Models.Commerce;
using Ecom.Infrastructure.Persistence.Database.UnitOfWork;
using Ecom.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Ecom.IntegrationTests.PostgreSql;

[Collection(PostgreSqlCollection.Name)]
public sealed class SePayIpnPostgreSqlTests(PostgreSqlFixture fixture)
{
    [PostgreSqlFact]
    public async Task Late_ipn_is_audited_for_reconciliation_without_marking_the_payment_paid()
    {
        await fixture.ResetDatabaseAsync();
        var now = DateTime.UtcNow;
        var seed = await SeedOrderAndAttemptAsync(now.AddMinutes(-31), now.AddMinutes(-1));

        var result = await ProcessIpnAsync(seed.Attempt.InvoiceNumber, seed.Payment.Amount, now, "late-transaction");

        Assert.True(result.IsSuccess);
        await using var verificationContext = fixture.CreateDbContext();
        var payment = await verificationContext.Payments.SingleAsync(x => x.Id == seed.Payment.Id);
        var attempt = await verificationContext.PaymentGatewayAttempts.SingleAsync(x => x.Id == seed.Attempt.Id);
        var notification = await verificationContext.PaymentGatewayNotifications.SingleAsync();
        Assert.Equal(PaymentStatus.Pending, payment.Status);
        Assert.Equal(PaymentGatewayAttemptStatus.NeedsReconciliation, attempt.Status);
        Assert.Equal(PaymentGatewayNotificationDisposition.NeedsReconciliation, notification.Disposition);
        Assert.Equal("LATE_PAYMENT", notification.FailureReasonCode);
        Assert.Empty(await verificationContext.PaymentTransactions.ToListAsync());
    }

    [PostgreSqlFact]
    public async Task Repeated_ipn_creates_one_payment_transaction_and_one_accepted_notification()
    {
        await fixture.ResetDatabaseAsync();
        var now = DateTime.UtcNow;
        var seed = await SeedOrderAndAttemptAsync(now, now.AddMinutes(30));

        var first = await ProcessIpnAsync(seed.Attempt.InvoiceNumber, seed.Payment.Amount, now, "duplicate-transaction");
        var duplicate = await ProcessIpnAsync(seed.Attempt.InvoiceNumber, seed.Payment.Amount, now, "duplicate-transaction");

        Assert.True(first.IsSuccess);
        Assert.True(duplicate.IsSuccess);
        await using var verificationContext = fixture.CreateDbContext();
        Assert.Equal(PaymentStatus.Paid, (await verificationContext.Payments.SingleAsync(x => x.Id == seed.Payment.Id)).Status);
        Assert.Single(await verificationContext.PaymentTransactions.ToListAsync());
        var notification = await verificationContext.PaymentGatewayNotifications.SingleAsync();
        Assert.Equal(PaymentGatewayNotificationDisposition.Accepted, notification.Disposition);
    }

    [PostgreSqlFact]
    public async Task Attempt_lock_blocks_a_second_ipn_transaction_for_the_same_invoice()
    {
        await fixture.ResetDatabaseAsync();
        var now = DateTime.UtcNow;
        var seed = await SeedOrderAndAttemptAsync(now, now.AddMinutes(30));

        await using var firstContext = fixture.CreateDbContext();
        await using var firstTransaction = await firstContext.Database.BeginTransactionAsync();
        var firstStore = new OrderLifecycleStore(firstContext);
        Assert.NotNull(await firstStore.LockPaymentGatewayAttemptAsync("sepay", seed.Attempt.InvoiceNumber, CancellationToken.None));

        await using var secondContext = fixture.CreateDbContext();
        await using var secondTransaction = await secondContext.Database.BeginTransactionAsync();
        await secondContext.Database.ExecuteSqlRawAsync("SET LOCAL lock_timeout = '100ms';");
        var secondStore = new OrderLifecycleStore(secondContext);

        var exception = await Assert.ThrowsAsync<PostgresException>(() => secondStore.LockPaymentGatewayAttemptAsync(
            "sepay", seed.Attempt.InvoiceNumber, CancellationToken.None));
        Assert.Equal(PostgresErrorCodes.LockNotAvailable, exception.SqlState);
    }

    private async Task<TResult> ProcessIpnAsync(string invoiceNumber, decimal amount, DateTime occurredAt,
        string externalTransactionId)
    {
        await using var context = fixture.CreateDbContext();
        using var unitOfWork = new UnitOfWork(context, NullLogger<UnitOfWork>.Instance);
        Assert.True(await unitOfWork.BeginTransactionAsync());
        var handler = new ProcessSePayIpnCommandHandler(unitOfWork, new OrderLifecycleStore(context), CreateSePayService());
        var result = await handler.Handle(new ProcessSePayIpnCommand("ipn-secret", CreateApprovedPayload(
            invoiceNumber, amount, occurredAt, externalTransactionId)), CancellationToken.None);
        if (result.IsSuccess)
            await unitOfWork.CommitTransactionAsync();
        else
            await unitOfWork.RollbackTransactionAsync();
        return result;
    }

    private async Task<SeededPayment> SeedOrderAndAttemptAsync(DateTime placedAt, DateTime dueAt)
    {
        var orderItems = new List<OrderItem>();
        var orderHistory = new List<OrderStatusHistory>();
        var order = Order.Create($"SEPAY-{Guid.NewGuid():N}", null, "sepay-test-guest", null, "0900000000",
            "SePay buyer", "0900000000", null, "SePay test address", 0m, placedAt,
            [new OrderLineSnapshot(null, "SePay product", "Default", "SEPAY-SKU", 100_000m, 1)], orderItems, orderHistory);
        var payment = Payment.Create(order.Id, PaymentMethod.SePay, order.GrandTotalAmount, dueAt);
        var attempt = PaymentGatewayAttempt.Create(payment.Id, "sepay", $"SP-{order.OrderNumber}", payment.Amount, "VND", dueAt);
        attempt.MarkCheckoutIssued(placedAt);

        await using var context = fixture.CreateDbContext();
        context.Orders.Add(order);
        context.OrderItems.AddRange(orderItems);
        context.OrderStatusHistories.AddRange(orderHistory);
        context.Payments.Add(payment);
        context.PaymentGatewayAttempts.Add(attempt);
        await context.SaveChangesAsync();
        return new SeededPayment(payment, attempt);
    }

    private static SePayIpnPayload CreateApprovedPayload(string invoiceNumber, decimal amount, DateTime occurredAt,
        string externalTransactionId) => new(
            0,
            "ORDER_PAID",
            new SePayIpnOrder("provider-order", "CAPTURED", "VND", amount.ToString("0.00", CultureInfo.InvariantCulture), invoiceNumber),
            new SePayIpnTransaction(externalTransactionId, "provider-reference", "APPROVED", "VND",
                amount.ToString("0.00", CultureInfo.InvariantCulture), occurredAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)));

    private static ISePayCheckoutService CreateSePayService() => new SePayCheckoutService(Options.Create(new SePayOptions
    {
        Enabled = true,
        MerchantId = "merchant-test",
        MerchantSecretKey = "merchant-secret",
        IpnSecretKey = "ipn-secret",
        CheckoutInitUrl = "https://pay-sandbox.sepay.vn/v1/checkout/init",
        PublicResultBaseUrl = "https://shop.example.test"
    }));

    private sealed record SeededPayment(Payment Payment, PaymentGatewayAttempt Attempt);
}
