using Ecom.Application.Common.Configuration;
using Microsoft.Extensions.Options;

namespace Ecom.Infrastructure.Security;

public sealed class SePayBankQrOptionsValidator : IValidateOptions<SePayBankQrOptions>
{
    public ValidateOptionsResult Validate(string? name, SePayBankQrOptions options)
    {
        if (!options.Enabled) return ValidateOptionsResult.Success;
        if (string.IsNullOrWhiteSpace(options.BankCode) || string.IsNullOrWhiteSpace(options.VirtualAccountNumber) ||
            string.IsNullOrWhiteSpace(options.AccountHolder) || string.IsNullOrWhiteSpace(options.PaymentCodePrefix) ||
            string.IsNullOrWhiteSpace(options.WebhookHmacSecret))
            return ValidateOptionsResult.Fail("SePayBankQr bank, virtual account, account holder, payment prefix and HMAC secret are required when enabled.");
        if (!Uri.TryCreate(options.WebhookUrl, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
            return ValidateOptionsResult.Fail("SePayBankQr:WebhookUrl must be absolute HTTPS when enabled.");
        return ValidateOptionsResult.Success;
    }
}
