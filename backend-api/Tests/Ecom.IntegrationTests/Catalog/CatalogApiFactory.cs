using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Ecom.IntegrationTests.Catalog;

internal sealed class CatalogApiFactory(PostgreSql.PostgreSqlFixture fixture) : WebApplicationFactory<Program>
{
    internal const string JwtIssuer = "ecom-integration-tests";
    internal const string JwtAudience = "ecom-integration-tests-client";
    internal const string JwtSecret = "ecom-integration-test-signing-key-000000000000000";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            var settings = new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = fixture.ConnectionString,
                ["Jwt:SecretKey"] = JwtSecret,
                ["Jwt:Issuer"] = JwtIssuer,
                ["Jwt:Audience"] = JwtAudience,
                ["Jwt:ValidateIssuer"] = "true",
                ["Jwt:ValidateAudience"] = "true",
                ["Jwt:ValidateLifetime"] = "true",
                ["Otp:HashKey"] = "ecom-integration-test-otp-hash-key-000000000000000",
                ["AuthenticationRateLimits:Backend"] = "InMemory",
                ["OpenTelemetry:Enabled"] = "false",
                ["Swagger:Enabled"] = "false",
                ["Outbox:Enabled"] = "false",
                ["MediaStorage:Provider"] = "Local"
            };
            configuration.AddInMemoryCollection(settings);
        });
    }
}
