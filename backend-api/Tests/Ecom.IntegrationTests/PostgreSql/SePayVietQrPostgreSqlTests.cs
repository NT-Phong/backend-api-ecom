using Ecom.Application.Common.Commerce;
using Ecom.Application.Common.Configuration;
using Ecom.Application.Common.Interfaces;
using Ecom.Application.Common.Models;
using Ecom.Application.Features.Commerce.Payments.Commands.CreateSePayVietQr;
using Ecom.Domain.Entities;
using Ecom.Domain.Enums;
using Ecom.Domain.Models.Commerce;
using Ecom.Infrastructure.Persistence.Database.UnitOfWork;
using Ecom.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Ecom.IntegrationTests.PostgreSql;

[Collection(PostgreSqlCollection.Name)]
public sealed class SePayVietQrPostgreSqlTests(PostgreSqlFixture fixture)
{
    [PostgreSqlFact]
    public async Task First_qr_issue_commits_an_insert_and_reopen_reuses_the_same_attempt()
    {
        await fixture.ResetDatabaseAsync();
        var seed = await SeedOrderAndPaymentAsync();

        var first = await CreateQrAsync(seed.Order.Id, seed.GuestTokenHash);
        var second = await CreateQrAsync(seed.Order.Id, seed.GuestTokenHash);

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        Assert.Equal(first.Data!.PaymentCode, second.Data!.PaymentCode);

        await using var verificationContext = fixture.CreateDbContext();
        var attempts = await verificationContext.PaymentBankQrAttempts
            .Where(x => x.PaymentId == seed.Payment.Id)
            .ToListAsync();
        var attempt = Assert.Single(attempts);
        Assert.Equal(PaymentBankQrAttemptStatus.QrIssued, attempt.Status);
        Assert.NotNull(attempt.QrIssuedAt);
    }

    private async Task<TResult<SePayVietQrDto>> CreateQrAsync(Guid orderId, string guestTokenHash)
    {
        await using var context = fixture.CreateDbContext();
        using var unitOfWork = new UnitOfWork(context, NullLogger<UnitOfWork>.Instance);
        Assert.True(await unitOfWork.BeginTransactionAsync());
        var handler = new CreateSePayVietQrCommandHandler(unitOfWork,
            new FixedPrincipalResolver(new CartPrincipal(null, guestTokenHash)), new OrderLifecycleStore(context), CreateService());
        var result = await handler.Handle(new CreateSePayVietQrCommand(orderId), CancellationToken.None);
        if (result.IsSuccess)
            await unitOfWork.CommitTransactionAsync();
        else
            await unitOfWork.RollbackTransactionAsync();
        return result;
    }

    private async Task<SeededPayment> SeedOrderAndPaymentAsync()
    {
        var now = DateTime.UtcNow;
        const string guestTokenHash = "sepay-vietqr-test-guest";
        var orderItems = new List<OrderItem>();
        var orderHistory = new List<OrderStatusHistory>();
        var order = Order.Create($"SEPAY-QR-{Guid.NewGuid():N}", null, guestTokenHash, null, "0900000000",
            "SePay buyer", "0900000000", null, "SePay test address", 0m, now,
            [new OrderLineSnapshot(null, "SePay product", "Default", "SEPAY-QR-SKU", 100_000m, 1)], orderItems, orderHistory);
        var payment = Payment.Create(order.Id, PaymentMethod.SePayVietQr, order.GrandTotalAmount, now.AddMinutes(30));

        await using var context = fixture.CreateDbContext();
        context.Orders.Add(order);
        context.OrderItems.AddRange(orderItems);
        context.OrderStatusHistories.AddRange(orderHistory);
        context.Payments.Add(payment);
        await context.SaveChangesAsync();
        return new SeededPayment(order, payment, guestTokenHash);
    }

    private static ISePayBankQrService CreateService() => new SePayBankQrService(Options.Create(new SePayBankQrOptions
    {
        Enabled = true,
        BankCode = "BIDV",
        VirtualAccountNumber = "1234567890",
        AccountHolder = "NGUYEN THANH PHONG",
        PaymentCodePrefix = "DH",
        WebhookHmacSecret = "webhook-secret",
        WebhookUrl = "https://api.example.test/api/v1/payments/sepay-bank/webhook"
    }));

    private sealed class FixedPrincipalResolver(CartPrincipal principal) : ICartPrincipalResolver
    {
        public CartPrincipal ResolveOrCreateGuestPrincipal() => principal;
        public CartPrincipal RotateGuestPrincipal() => principal;
        public CartPrincipal? ResolveExistingPrincipal() => principal;
        public CartPrincipal? ResolveGuestPrincipal() => principal;
        public void ClearGuestPrincipal() { }
    }

    private sealed record SeededPayment(Order Order, Payment Payment, string GuestTokenHash);
}
