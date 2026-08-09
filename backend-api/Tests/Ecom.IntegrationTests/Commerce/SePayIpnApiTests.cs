using System.Net;
using System.Text;
using Ecom.IntegrationTests.PostgreSql;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Ecom.IntegrationTests.Commerce;

[Collection(PostgreSqlCollection.Name)]
public sealed class SePayIpnApiTests(PostgreSqlFixture fixture)
{
    [PostgreSqlFact]
    public async Task Ipn_rejects_an_invalid_secret_before_processing_the_payload()
    {
        await fixture.ResetDatabaseAsync();
        await using var factory = new SePayApiFactory(fixture);
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/payments/sepay/ipn")
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        };
        request.Headers.Add("X-Secret-Key", "wrong-secret");

        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [PostgreSqlFact]
    public async Task Ipn_rejects_json_bodies_larger_than_16_kib()
    {
        await fixture.ResetDatabaseAsync();
        await using var factory = new SePayApiFactory(fixture);
        using var client = factory.CreateClient();
        var oversizedPayload = "{\"padding\":\"" + new string('x', 16 * 1024) + "\"}";
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/payments/sepay/ipn")
        {
            Content = new StringContent(oversizedPayload, Encoding.UTF8, "application/json")
        };
        request.Headers.Add("X-Secret-Key", "ipn-secret");

        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
    }

    private sealed class SePayApiFactory(PostgreSqlFixture fixture) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = fixture.ConnectionString,
                    ["Jwt:SecretKey"] = "ecom-integration-test-signing-key-000000000000000",
                    ["Otp:HashKey"] = "ecom-integration-test-otp-hash-key-000000000000000",
                    ["AuthenticationRateLimits:Backend"] = "InMemory",
                    ["Cors:AllowedOrigins:0"] = "https://shop.example.test",
                    ["OpenTelemetry:Enabled"] = "false",
                    ["Swagger:Enabled"] = "false",
                    ["Outbox:Enabled"] = "false",
                    ["MediaStorage:Provider"] = "Local",
                    ["SePay:Enabled"] = "true",
                    ["SePay:Environment"] = "Sandbox",
                    ["SePay:MerchantId"] = "merchant-test",
                    ["SePay:MerchantSecretKey"] = "merchant-secret",
                    ["SePay:IpnSecretKey"] = "ipn-secret",
                    ["SePay:IpnAuthenticationMode"] = "SecretKey",
                    ["SePay:CheckoutInitUrl"] = "https://pay-sandbox.sepay.vn/v1/checkout/init",
                    ["SePay:PublicResultBaseUrl"] = "https://shop.example.test"
                }));
        }
    }
}
