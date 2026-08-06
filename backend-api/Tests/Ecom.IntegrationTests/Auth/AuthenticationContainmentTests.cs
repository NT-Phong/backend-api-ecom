using Ecom.Application.Common.Configuration;
using Ecom.Application.Common.Interfaces;
using Ecom.Domain.Enums;
using Ecom.Domain.Constants;
using Ecom.Infrastructure.Logging;
using Ecom.Infrastructure.Security;
using Ecom.Infrastructure.Services.Sms;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog.Events;
using Serilog.Parsing;
using System.Collections.Concurrent;

namespace Ecom.IntegrationTests.Auth;

public sealed class AuthenticationContainmentTests
{
    private const string OtpHashKey = "integration-test-otp-hash-key-32-characters";

    [Fact]
    public void Otp_is_generated_with_expected_shape_and_stored_as_purpose_bound_digest()
    {
        var service = CreateOtpService(Environments.Production);
        var userId = Guid.NewGuid();
        var code = service.GenerateCode();
        var protectedCode = service.Protect(userId, OtpTokenTypeEnum.Login, code);

        Assert.Matches("^[0-9]{4}$", code);
        Assert.Equal(10, protectedCode.Length);
        Assert.NotEqual(code, protectedCode);
        Assert.True(service.Verify(userId, OtpTokenTypeEnum.Login, code, protectedCode));
        Assert.False(service.Verify(userId, OtpTokenTypeEnum.ActivateAccount, code, protectedCode));
        Assert.False(service.Verify(Guid.NewGuid(), OtpTokenTypeEnum.Login, code, protectedCode));
    }

    [Fact]
    public void Production_rejects_development_test_account_configuration()
    {
        var validator = new OtpSettingsValidator(new TestHostEnvironment(Environments.Production));
        var result = validator.Validate(null, new OtpSettings
        {
            HashKey = OtpHashKey,
            EnableDevelopmentTestAccounts = true,
            EnableDevelopmentFixedOtp = true,
            ExposeDevelopmentOtp = true
        });

        Assert.True(result.Failed);
        Assert.Contains(result.Failures!, failure => failure.Contains("cannot be enabled", StringComparison.Ordinal));
    }

    [Fact]
    public void Development_test_account_requires_both_environment_and_explicit_option()
    {
        var disabled = CreateOtpService(Environments.Development, enableTestAccounts: false);
        var enabled = CreateOtpService(Environments.Development, enableTestAccounts: true);
        var production = CreateOtpService(Environments.Production, enableTestAccounts: true);

        Assert.False(disabled.IsDevelopmentTestAccount(TestAccounts.Manager));
        Assert.True(enabled.IsDevelopmentTestAccount(TestAccounts.Manager));
        Assert.False(production.IsDevelopmentTestAccount(TestAccounts.Manager));
    }

    [Fact]
    public void Development_fixed_otp_accepts_any_valid_phone_without_a_sms_provider()
    {
        var service = CreateOtpService(Environments.Development, enableFixedOtp: true);

        Assert.True(service.IsDevelopmentTestAccount("0912345678"));
        Assert.True(service.CanExposeDevelopmentOtp);
        Assert.Equal("0000", service.DevelopmentOtp);
    }

    [Fact]
    public void Development_configured_test_phone_narrows_fixed_otp_bypass_to_that_phone()
    {
        var service = CreateOtpService(
            Environments.Development,
            enableTestAccounts: true,
            testPhoneNumber: TestAccounts.Admin);

        Assert.True(service.IsDevelopmentTestAccount(TestAccounts.Admin));
        Assert.False(service.IsDevelopmentTestAccount(TestAccounts.Manager));
        Assert.True(service.CanExposeDevelopmentOtp);
    }

    [Fact]
    public async Task Missing_sms_provider_is_explicitly_fail_closed()
    {
        var sender = new SmsSender();
        Assert.False(sender.IsConfigured);

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await sender.SendAsync("redacted", "redacted", 1));
    }

    [Fact]
    public void Refresh_token_protection_is_deterministic_and_does_not_store_raw_token()
    {
        var protector = new AuthTokenProtector();
        var raw = "a-high-entropy-refresh-token-value";
        var protectedToken = protector.Protect(raw);

        Assert.NotEqual(raw, protectedToken);
        Assert.DoesNotContain(raw, protectedToken, StringComparison.Ordinal);
        Assert.Equal(protectedToken, protector.Protect(raw));
        Assert.True(protector.IsProtected(protectedToken));
    }

    [Fact]
    public void Http_log_enrichment_excludes_query_credentials_and_raw_forwarded_addresses()
    {
        const string credential = "raw-access-token-that-must-not-be-logged";
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Get;
        context.Request.Path = "/hubs/notification";
        context.Request.QueryString = new QueryString($"?access_token={credential}");
        context.Request.Headers["X-Forwarded-For"] = "203.0.113.42";
        HttpContextEnricher.SetHttpContextAccessor(new HttpContextAccessor { HttpContext = context });

        var logEvent = new LogEvent(
            DateTimeOffset.UtcNow,
            LogEventLevel.Information,
            null,
            new MessageTemplateParser().Parse("request"),
            []);
        new HttpContextEnricher().Enrich(logEvent, new TestLogEventPropertyFactory());
        var serialized = string.Join("|", logEvent.Properties.Select(pair => $"{pair.Key}={pair.Value}"));

        Assert.DoesNotContain(credential, serialized, StringComparison.Ordinal);
        Assert.DoesNotContain("203.0.113.42", serialized, StringComparison.Ordinal);
        Assert.True(logEvent.Properties.TryGetValue("Endpoint", out var endpoint));
        Assert.Equal("GET /hubs/notification", Assert.IsType<ScalarValue>(endpoint).Value);
    }

    [Fact]
    public void Json_log_formatter_redacts_auth_credentials_and_contact_pii()
    {
        const string otp = "7391";
        const string token = "raw-refresh-token";
        const string phone = "0912345678";
        const string email = "person@example.test";
        var template = new MessageTemplateParser().Parse(
            "otp: {Otp} refresh_token: {RefreshToken} phone: {Phone} email: {Email}");
        var logEvent = new LogEvent(
            DateTimeOffset.UtcNow,
            LogEventLevel.Error,
            new InvalidOperationException($"refresh_token: {token}"),
            template,
            [
                new LogEventProperty("Otp", new ScalarValue(otp)),
                new LogEventProperty("RefreshToken", new ScalarValue(token)),
                new LogEventProperty("Phone", new ScalarValue(phone)),
                new LogEventProperty("Email", new ScalarValue(email))
            ]);
        using var writer = new StringWriter();

        new CustomJsonFormatter().Format(logEvent, writer);
        var output = writer.ToString();

        Assert.DoesNotContain(otp, output, StringComparison.Ordinal);
        Assert.DoesNotContain(token, output, StringComparison.Ordinal);
        Assert.DoesNotContain(phone, output, StringComparison.Ordinal);
        Assert.DoesNotContain(email, output, StringComparison.Ordinal);
        Assert.DoesNotContain("InvalidOperationException:", output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Distributed_limit_rejects_same_partition_but_keeps_other_partitions_independent()
    {
        var store = new ControllableCounterStore();
        var limiter = CreateLimiter(store);

        for (var i = 0; i < 3; i++)
            Assert.Equal(AuthRateLimitStatus.Allowed, (await limiter.AcquireAsync(
                AuthRateLimitPolicyNames.OtpSendDestinationBurst, "destination-a")).Status);

        Assert.Equal(AuthRateLimitStatus.Rejected, (await limiter.AcquireAsync(
            AuthRateLimitPolicyNames.OtpSendDestinationBurst, "destination-a")).Status);
        Assert.Equal(AuthRateLimitStatus.Allowed, (await limiter.AcquireAsync(
            AuthRateLimitPolicyNames.OtpSendDestinationBurst, "destination-b")).Status);
    }

    [Fact]
    public async Task Otp_verify_allows_five_attempts_per_challenge_and_new_challenge_gets_new_partition()
    {
        var limiter = CreateLimiter(new ControllableCounterStore());

        for (var attempt = 0; attempt < 5; attempt++)
            Assert.Equal(AuthRateLimitStatus.Allowed, (await limiter.AcquireAsync(
                AuthRateLimitPolicyNames.OtpVerifyChallenge, "otp-id:protected-code-v1")).Status);

        Assert.Equal(AuthRateLimitStatus.Rejected, (await limiter.AcquireAsync(
            AuthRateLimitPolicyNames.OtpVerifyChallenge, "otp-id:protected-code-v1")).Status);
        Assert.Equal(AuthRateLimitStatus.Allowed, (await limiter.AcquireAsync(
            AuthRateLimitPolicyNames.OtpVerifyChallenge, "otp-id:protected-code-v2")).Status);
    }

    [Fact]
    public async Task Distributed_limit_resets_after_expiry_and_is_shared_across_instances()
    {
        var store = new ControllableCounterStore();
        var firstInstance = CreateLimiter(store);
        var secondInstance = CreateLimiter(store);

        Assert.Equal(AuthRateLimitStatus.Allowed, (await firstInstance.AcquireAsync(
            AuthRateLimitPolicyNames.RegisterDestinationDaily, "same-destination")).Status);
        Assert.Equal(AuthRateLimitStatus.Allowed, (await secondInstance.AcquireAsync(
            AuthRateLimitPolicyNames.RegisterDestinationDaily, "same-destination")).Status);
        Assert.Equal(AuthRateLimitStatus.Allowed, (await firstInstance.AcquireAsync(
            AuthRateLimitPolicyNames.RegisterDestinationDaily, "same-destination")).Status);
        Assert.Equal(AuthRateLimitStatus.Rejected, (await secondInstance.AcquireAsync(
            AuthRateLimitPolicyNames.RegisterDestinationDaily, "same-destination")).Status);

        store.Advance(TimeSpan.FromDays(1).Add(TimeSpan.FromSeconds(1)));
        Assert.Equal(AuthRateLimitStatus.Allowed, (await secondInstance.AcquireAsync(
            AuthRateLimitPolicyNames.RegisterDestinationDaily, "same-destination")).Status);
    }

    [Fact]
    public async Task Redis_unavailable_result_is_fail_closed()
    {
        IAuthRateLimitService limiter = new UnavailableAuthRateLimitService();
        var result = await limiter.AcquireAsync(
            AuthRateLimitPolicyNames.RefreshSession,
            "session-fingerprint");

        Assert.Equal(AuthRateLimitStatus.Unavailable, result.Status);
    }

    private static OtpSecurityService CreateOtpService(
        string environment,
        bool enableTestAccounts = false,
        bool enableFixedOtp = false,
        string? testPhoneNumber = null) =>
        new(
            Options.Create(new OtpSettings
            {
                OtpLength = 4,
                HashKey = OtpHashKey,
                DefaultOtp = "0000",
                TestPhoneNumber = testPhoneNumber ?? string.Empty,
                EnableDevelopmentTestAccounts = enableTestAccounts,
                EnableDevelopmentFixedOtp = enableFixedOtp,
                ExposeDevelopmentOtp = enableTestAccounts
                    || enableFixedOtp
            }),
            new TestHostEnvironment(environment));

    private static DistributedAuthRateLimitService CreateLimiter(IAuthRateLimitCounterStore store) =>
        new(store, Options.Create(new AuthRateLimitOptions()));

    private sealed class TestHostEnvironment(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;
        public string ApplicationName { get; set; } = "AuthenticationTests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }

    private sealed class ControllableCounterStore : IAuthRateLimitCounterStore
    {
        private readonly ConcurrentDictionary<string, Entry> _entries = new();
        private DateTimeOffset _now = DateTimeOffset.UnixEpoch;

        public void Advance(TimeSpan duration) => _now += duration;

        public Task<AuthRateLimitCounter?> IncrementAsync(
            string key,
            TimeSpan window,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var entry = _entries.AddOrUpdate(
                key,
                _ => new Entry(1, _now + window),
                (_, current) => current.ExpiresAt <= _now
                    ? new Entry(1, _now + window)
                    : current with { Count = current.Count + 1 });
            return Task.FromResult<AuthRateLimitCounter?>(
                new AuthRateLimitCounter(entry.Count, entry.ExpiresAt - _now));
        }

        private sealed record Entry(long Count, DateTimeOffset ExpiresAt);
    }

    private sealed class TestLogEventPropertyFactory : Serilog.Core.ILogEventPropertyFactory
    {
        public LogEventProperty CreateProperty(string name, object? value, bool destructureObjects = false) =>
            new(name, new ScalarValue(value));
    }
}
