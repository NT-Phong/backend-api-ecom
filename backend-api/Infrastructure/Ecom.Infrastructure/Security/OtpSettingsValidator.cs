using Ecom.Application.Common.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Ecom.Infrastructure.Security;

public sealed class OtpSettingsValidator(IHostEnvironment environment) : IValidateOptions<OtpSettings>
{
    public ValidateOptionsResult Validate(string? name, OtpSettings options)
    {
        var failures = new List<string>();

        if (options.OtpLength is < 4 or > 9)
            failures.Add("Otp:OtpLength must be between 4 and 9.");
        if (options.MaxAttempts is < 1 or > 10)
            failures.Add("Otp:MaxAttempts must be between 1 and 10.");
        if (string.IsNullOrWhiteSpace(options.HashKey) || options.HashKey.Length < 32)
            failures.Add("Otp:HashKey must be supplied by configuration and contain at least 32 characters.");
        if (!environment.IsDevelopment() &&
            (options.EnableDevelopmentTestAccounts || options.EnableDevelopmentFixedOtp || options.ExposeDevelopmentOtp))
        {
            failures.Add("Development OTP/test-account options cannot be enabled outside Development.");
        }
        if (options.ExposeDevelopmentOtp && !options.EnableDevelopmentTestAccounts && !options.EnableDevelopmentFixedOtp)
            failures.Add("Otp:ExposeDevelopmentOtp requires a Development OTP mode.");
        if ((options.EnableDevelopmentTestAccounts || options.EnableDevelopmentFixedOtp) &&
            (options.DefaultOtp.Length != options.OtpLength || !options.DefaultOtp.All(char.IsDigit)))
        {
            failures.Add("Otp:DefaultOtp must be numeric and match Otp:OtpLength when a Development OTP mode is enabled.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
