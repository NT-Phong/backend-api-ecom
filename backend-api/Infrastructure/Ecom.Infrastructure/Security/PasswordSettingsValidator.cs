using Ecom.Application.Common.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Ecom.Infrastructure.Security;

public sealed class PasswordSettingsValidator(IHostEnvironment environment) : IValidateOptions<PasswordSettings>
{
    public ValidateOptionsResult Validate(string? name, PasswordSettings settings)
    {
        if (settings.MinLength is < 5 or > 128)
            return ValidateOptionsResult.Fail("Password:MinLength must be between 5 and 128.");
        if (!environment.IsDevelopment() && settings.MinLength < 15)
            return ValidateOptionsResult.Fail("Password:MinLength must be at least 15 outside Development.");
        return ValidateOptionsResult.Success;
    }
}
