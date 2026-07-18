using Ecom.Application.Common.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Ecom.Infrastructure.Security;

public sealed class PasswordAuthenticationV2OptionsValidator(IHostEnvironment environment)
    : IValidateOptions<PasswordAuthenticationV2Options>
{
    public ValidateOptionsResult Validate(string? name, PasswordAuthenticationV2Options options)
    {
        if (options.ExposeDevelopmentVerificationToken && !environment.IsDevelopment())
            return ValidateOptionsResult.Fail("Development verification tokens may only be exposed in Development.");
        if (options.AccessTokenMinutes is < 1 or > 15)
            return ValidateOptionsResult.Fail("AccessTokenMinutes must be between 1 and 15.");
        if (options.RefreshTokenDays is < 1 or > 90)
            return ValidateOptionsResult.Fail("RefreshTokenDays must be between 1 and 90.");
        return ValidateOptionsResult.Success;
    }
}
