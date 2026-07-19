using Ecom.Application.Common.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Ecom.Infrastructure.Security;

public sealed class DemoQrLoginOptionsValidator(IHostEnvironment environment)
    : IValidateOptions<DemoQrLoginOptions>
{
    public ValidateOptionsResult Validate(string? name, DemoQrLoginOptions options)
    {
        if (options.Enabled && !environment.IsDevelopment())
            return ValidateOptionsResult.Fail("Demo QR login may only be enabled in Development.");
        if (options.TtlSeconds is < 30 or > 300)
            return ValidateOptionsResult.Fail("DemoQrLogin:TtlSeconds must be between 30 and 300.");
        if (options.PollIntervalMilliseconds is < 500 or > 5000)
            return ValidateOptionsResult.Fail("DemoQrLogin:PollIntervalMilliseconds must be between 500 and 5000.");

        return ValidateOptionsResult.Success;
    }
}
