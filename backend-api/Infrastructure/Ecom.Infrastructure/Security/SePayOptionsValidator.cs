using Ecom.Application.Common.Configuration;
using Microsoft.Extensions.Options;

namespace Ecom.Infrastructure.Security;

public sealed class SePayOptionsValidator : IValidateOptions<SePayOptions>
{
    public ValidateOptionsResult Validate(string? name, SePayOptions options)
    {
        if (!options.Enabled)
            return ValidateOptionsResult.Success;

        if (!string.Equals(options.Environment, "Sandbox", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(options.Environment, "Production", StringComparison.OrdinalIgnoreCase))
            return ValidateOptionsResult.Fail("SePay:Environment must be Sandbox or Production.");

        if (string.IsNullOrWhiteSpace(options.MerchantId) || string.IsNullOrWhiteSpace(options.MerchantSecretKey) ||
            string.IsNullOrWhiteSpace(options.IpnSecretKey))
            return ValidateOptionsResult.Fail("SePay merchant and IPN secrets are required when SePay is enabled.");

        if (!string.Equals(options.IpnAuthenticationMode, "SecretKey", StringComparison.OrdinalIgnoreCase))
            return ValidateOptionsResult.Fail("SePay V1 requires IpnAuthenticationMode=SecretKey.");

        var expectedCheckoutUrl = string.Equals(options.Environment, "Sandbox", StringComparison.OrdinalIgnoreCase)
            ? "https://pay-sandbox.sepay.vn/v1/checkout/init"
            : "https://pay.sepay.vn/v1/checkout/init";
        if (!string.Equals(options.CheckoutInitUrl?.TrimEnd('/'), expectedCheckoutUrl, StringComparison.OrdinalIgnoreCase) ||
            !IsAbsoluteHttps(options.PublicResultBaseUrl))
            return ValidateOptionsResult.Fail("SePay checkout URL must match the selected environment and the public result URL must be absolute HTTPS.");

        return ValidateOptionsResult.Success;
    }

    private static bool IsAbsoluteHttps(string value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps;
}
