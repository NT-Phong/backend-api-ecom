using Ecom.Application.Common.Configuration;
using Ecom.Application.Features.AuthV2.Login;
using Ecom.Application.Features.AuthV2.Register;
using Ecom.Application.Features.Auth.Commands.UpdateBasicProfile;
using Ecom.Infrastructure.Security;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Ecom.IntegrationTests.Auth;

public sealed class PasswordAuthenticationV2Tests
{
    [Fact]
    public void Production_rejects_development_verification_token_exposure()
    {
        var validator = new PasswordAuthenticationV2OptionsValidator(new EnvironmentStub(Environments.Production));
        var result = validator.Validate(null, new PasswordAuthenticationV2Options
        { Enabled = true, ExposeDevelopmentVerificationToken = true });
        Assert.True(result.Failed);
    }

    [Theory]
    [InlineData("admin")]
    [InlineData("123456")]
    [InlineData("buyer@example")]
    public void Register_rejects_invalid_or_reserved_username(string username)
    {
        var result = new RegisterPasswordCommandValidator(Options.Create(new PasswordSettings { MinLength = 15 })).Validate(
            new RegisterPasswordCommand(username, "buyer@example.com", "a sufficiently long passphrase"));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Register_requires_fifteen_character_password_without_trimming()
    {
        var validator = new RegisterPasswordCommandValidator(Options.Create(new PasswordSettings { MinLength = 15 }));
        Assert.False(validator.Validate(new RegisterPasswordCommand("buyer.one", "buyer@example.com", new string('x', 14))).IsValid);
        Assert.True(validator.Validate(new RegisterPasswordCommand("buyer.one", "buyer@example.com", "a sufficiently long passphrase")).IsValid);
    }

    [Fact]
    public void Development_allows_a_five_character_password_but_production_rejects_the_setting()
    {
        var development = new PasswordSettingsValidator(new EnvironmentStub(Environments.Development));
        var production = new PasswordSettingsValidator(new EnvironmentStub(Environments.Production));
        var settings = new PasswordSettings { MinLength = 5 };
        Assert.False(development.Validate(null, settings).Failed);
        Assert.True(production.Validate(null, settings).Failed);
        var validator = new RegisterPasswordCommandValidator(Options.Create(settings));
        Assert.True(validator.Validate(new RegisterPasswordCommand("buyer.one", "buyer@example.com", "12345")).IsValid);
    }

    [Fact]
    public void Login_contract_is_mobile_only_and_rejects_overlong_password()
    {
        var validator = new PasswordLoginCommandValidator();
        Assert.False(validator.Validate(new PasswordLoginCommand("buyer.one", new string('x', 129), "device", false)).IsValid);
    }

    [Fact]
    public void Basic_profile_requires_a_non_blank_name()
    {
        var validator = new UpdateBasicProfileCommandValidator();
        Assert.False(validator.Validate(new UpdateBasicProfileCommand("  ")).IsValid);
        Assert.True(validator.Validate(new UpdateBasicProfileCommand("Nguyen Van A")).IsValid);
    }

    private sealed class EnvironmentStub(string name) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = name;
        public string ApplicationName { get; set; } = "tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
