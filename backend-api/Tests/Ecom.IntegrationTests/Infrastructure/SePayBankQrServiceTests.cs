using System.Security.Cryptography;
using System.Text;
using Ecom.Application.Common.Configuration;
using Ecom.Infrastructure.Security;
using Ecom.Infrastructure.Services;
using Microsoft.Extensions.Options;

namespace Ecom.IntegrationTests.Infrastructure;

public sealed class SePayBankQrServiceTests
{
    [Fact]
    public void Vietqr_form_uses_server_owned_virtual_account_amount_and_payment_code()
    {
        var service = CreateService();

        var form = service.CreateQrForm(125_000m, "DHABC123", new DateTime(2026, 8, 9, 12, 0, 0, DateTimeKind.Utc));

        Assert.Equal("BIDV", form.BankCode);
        Assert.Equal("1234567890", form.VirtualAccountDisplay);
        Assert.Equal("NGUYEN THANH PHONG", form.AccountHolder);
        Assert.Equal(125_000m, form.Amount);
        Assert.Equal("DHABC123", form.PaymentCode);
        Assert.StartsWith("https://vietqr.app/img?", form.QrImageUrl);
        Assert.Contains("acc=1234567890", form.QrImageUrl);
        Assert.Contains("amount=125000", form.QrImageUrl);
        Assert.Contains("des=DHABC123", form.QrImageUrl);
    }

    [Fact]
    public void Webhook_signature_and_virtual_account_use_exact_fixed_values()
    {
        var service = CreateService();
        const long timestamp = 1_786_276_800;
        const string rawBody = "{\"id\":1001}";
        var signature = CreateSignature("webhook-secret", timestamp, rawBody);

        // The timestamp is deliberately made current so the replay-window check is exercised separately from the HMAC.
        var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var currentSignature = CreateSignature("webhook-secret", currentTimestamp, rawBody);
        Assert.True(service.IsValidWebhookSignature(currentTimestamp.ToString(), rawBody, currentSignature));
        Assert.False(service.IsValidWebhookSignature(currentTimestamp.ToString(), rawBody + " ", currentSignature));
        Assert.False(service.IsValidWebhookSignature(currentTimestamp.ToString(), rawBody, signature));
        Assert.True(service.IsExpectedVirtualAccount("1234567890"));
        Assert.False(service.IsExpectedVirtualAccount("1234567891"));
    }

    [Fact]
    public void Enabled_bank_qr_requires_https_webhook_and_hmac_secret()
    {
        var validator = new SePayBankQrOptionsValidator();
        var options = new SePayBankQrOptions
        {
            Enabled = true,
            BankCode = "BIDV",
            VirtualAccountNumber = "1234567890",
            AccountHolder = "NGUYEN THANH PHONG",
            PaymentCodePrefix = "DH",
            WebhookUrl = "https://api.example.test/api/v1/payments/sepay-bank/webhook"
        };
        Assert.True(validator.Validate(null, options).Failed);
        options.WebhookHmacSecret = "webhook-secret";
        Assert.True(validator.Validate(null, options).Succeeded);
    }

    private static SePayBankQrService CreateService() => new(Options.Create(new SePayBankQrOptions
    {
        Enabled = true,
        BankCode = "BIDV",
        VirtualAccountNumber = "1234567890",
        AccountHolder = "NGUYEN THANH PHONG",
        PaymentCodePrefix = "DH",
        WebhookHmacSecret = "webhook-secret",
        WebhookUrl = "https://api.example.test/api/v1/payments/sepay-bank/webhook"
    }));

    private static string CreateSignature(string secret, long timestamp, string rawBody)
    {
        var hash = HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes($"{timestamp}.{rawBody}"));
        return "sha256=" + Convert.ToHexString(hash).ToLowerInvariant();
    }
}
