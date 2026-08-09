using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Ecom.Application.Common.Configuration;
using Ecom.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace Ecom.Infrastructure.Services;

public sealed class SePayCheckoutService(IOptions<SePayOptions> options) : ISePayCheckoutService
{
    private static readonly string[] SignedFields =
    [
        "order_amount", "merchant", "currency", "operation", "order_description", "order_invoice_number",
        "customer_id", "payment_method", "success_url", "error_url", "cancel_url"
    ];

    private SePayOptions Options => options.Value;
    public bool IsEnabled => Options.Enabled;

    public SePayCheckoutForm CreateCheckoutForm(SePayCheckoutRequest request)
    {
        if (!IsEnabled)
            throw new InvalidOperationException("SePay is not enabled.");
        if (request.OrderId == Guid.Empty || string.IsNullOrWhiteSpace(request.InvoiceNumber) || request.Amount <= 0)
            throw new ArgumentException("SePay checkout request is invalid.", nameof(request));

        var baseUrl = Options.PublicResultBaseUrl.TrimEnd('/');
        var fields = new List<SePayCheckoutField>
        {
            new("order_amount", request.Amount.ToString("0.00", CultureInfo.InvariantCulture)),
            new("merchant", Options.MerchantId),
            new("currency", "VND"),
            new("operation", "PURCHASE"),
            new("order_description", $"Thanh toan don hang {request.OrderNumber}"),
            new("order_invoice_number", request.InvoiceNumber)
        };
        if (request.CustomerId.HasValue)
            fields.Add(new("customer_id", request.CustomerId.Value.ToString("N")));

        fields.Add(new("success_url", $"{baseUrl}/orders/{request.OrderId:D}?payment=success"));
        fields.Add(new("error_url", $"{baseUrl}/orders/{request.OrderId:D}?payment=error"));
        fields.Add(new("cancel_url", $"{baseUrl}/orders/{request.OrderId:D}?payment=cancel"));
        fields.Add(new("signature", CreateSignature(fields, Options.MerchantSecretKey)));
        return new SePayCheckoutForm(Options.CheckoutInitUrl, "POST", fields);
    }

    public bool IsValidIpnSecret(string? suppliedSecret)
    {
        if (!IsEnabled || string.IsNullOrWhiteSpace(suppliedSecret) || string.IsNullOrWhiteSpace(Options.IpnSecretKey))
            return false;

        var expected = Encoding.UTF8.GetBytes(Options.IpnSecretKey);
        var supplied = Encoding.UTF8.GetBytes(suppliedSecret);
        return expected.Length == supplied.Length && CryptographicOperations.FixedTimeEquals(expected, supplied);
    }

    private static string CreateSignature(IReadOnlyList<SePayCheckoutField> fields, string secret)
    {
        var values = fields.ToDictionary(x => x.Name, x => x.Value, StringComparer.Ordinal);
        var canonical = string.Join(',', SignedFields
            .Where(values.ContainsKey)
            .Select(field => $"{field}={values[field]}"));
        var signature = HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(canonical));
        return Convert.ToBase64String(signature);
    }
}
