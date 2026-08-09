using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Ecom.Application.Common.Configuration;
using Ecom.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace Ecom.Infrastructure.Services;

public sealed class SePayBankQrService(IOptions<SePayBankQrOptions> options) : ISePayBankQrService
{
    private SePayBankQrOptions Options => options.Value;
    public bool IsEnabled => Options.Enabled;
    public string PaymentCodePrefix => Options.PaymentCodePrefix.Trim().ToUpperInvariant();
    public string VirtualAccountFingerprint => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(Options.VirtualAccountNumber))).ToLowerInvariant();

    public SePayVietQrForm CreateQrForm(decimal amount, string paymentCode, DateTime expiresAt)
    {
        if (!IsEnabled || amount <= 0 || string.IsNullOrWhiteSpace(paymentCode) || expiresAt == default)
            throw new InvalidOperationException("SePay Bank QR is not configured.");
        var query = new Dictionary<string, string>
        {
            ["acc"] = Options.VirtualAccountNumber,
            ["bank"] = Options.BankCode,
            ["amount"] = amount.ToString("0", CultureInfo.InvariantCulture),
            ["des"] = paymentCode,
            ["template"] = Options.QrTemplate
        };
        var url = "https://vietqr.app/img?" + string.Join('&', query.Select(x => $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value)}"));
        return new SePayVietQrForm(url, Options.BankCode, Options.VirtualAccountNumber, Options.AccountHolder,
            amount, "VND", paymentCode, expiresAt);
    }

    public bool IsValidWebhookSignature(string? timestamp, string rawBody, string? suppliedSignature)
    {
        if (!IsEnabled || !long.TryParse(timestamp, out var seconds) || string.IsNullOrWhiteSpace(rawBody) ||
            string.IsNullOrWhiteSpace(suppliedSignature) || string.IsNullOrWhiteSpace(Options.WebhookHmacSecret)) return false;
        var at = DateTimeOffset.FromUnixTimeSeconds(seconds);
        if (Math.Abs((DateTimeOffset.UtcNow - at).TotalMinutes) > 5) return false;
        var signed = Encoding.UTF8.GetBytes($"{seconds}.{rawBody}");
        var hash = HMACSHA256.HashData(Encoding.UTF8.GetBytes(Options.WebhookHmacSecret), signed);
        var expected = "sha256=" + Convert.ToHexString(hash).ToLowerInvariant();
        var supplied = Encoding.UTF8.GetBytes(suppliedSignature);
        var bytes = Encoding.UTF8.GetBytes(expected);
        return supplied.Length == bytes.Length && CryptographicOperations.FixedTimeEquals(bytes, supplied);
    }

    public bool IsExpectedVirtualAccount(string? suppliedAccountNumber)
    {
        if (string.IsNullOrWhiteSpace(suppliedAccountNumber) || string.IsNullOrWhiteSpace(Options.VirtualAccountNumber))
            return false;
        var expected = Encoding.UTF8.GetBytes(VirtualAccountFingerprint);
        var supplied = Encoding.UTF8.GetBytes(Convert.ToHexString(SHA256.HashData(
            Encoding.UTF8.GetBytes(suppliedAccountNumber.Trim()))).ToLowerInvariant());
        return supplied.Length == expected.Length && CryptographicOperations.FixedTimeEquals(expected, supplied);
    }
}
