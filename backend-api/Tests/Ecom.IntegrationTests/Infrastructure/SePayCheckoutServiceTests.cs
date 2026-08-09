using Ecom.Application.Common.Configuration;
using Ecom.Application.Common.Interfaces;
using Ecom.Infrastructure.Services;
using Ecom.Infrastructure.Security;
using Microsoft.Extensions.Options;

namespace Ecom.IntegrationTests.Infrastructure;

public sealed class SePayCheckoutServiceTests
{
    [Fact]
    public void Checkout_form_is_server_signed_and_ipn_secret_uses_exact_value()
    {
        var service = new SePayCheckoutService(Options.Create(new SePayOptions
        {
            Enabled = true,
            MerchantId = "merchant-test",
            MerchantSecretKey = "merchant-secret",
            IpnSecretKey = "ipn-secret-key",
            CheckoutInitUrl = "https://pay-sandbox.sepay.vn/v1/checkout/init",
            PublicResultBaseUrl = "https://shop.example.test"
        }));

        var form = service.CreateCheckoutForm(new SePayCheckoutRequest(Guid.Parse("11111111-1111-1111-1111-111111111111"),
            "SP-ORD-001", 100_000m, "ORD-001", Guid.Parse("22222222-2222-2222-2222-222222222222")));

        Assert.Equal("https://pay-sandbox.sepay.vn/v1/checkout/init", form.ActionUrl);
        Assert.Equal("POST", form.Method);
        Assert.Equal(new[]
        {
            "order_amount", "merchant", "currency", "operation", "order_description", "order_invoice_number",
            "customer_id", "success_url", "error_url", "cancel_url", "signature"
        }, form.Fields.Select(x => x.Name));
        Assert.Equal("100000.00", form.Fields[0].Value);
        Assert.Equal("merchant-test", form.Fields[1].Value);
        Assert.Equal("VND", form.Fields[2].Value);
        Assert.Equal("PURCHASE", form.Fields[3].Value);
        Assert.Equal("SP-ORD-001", form.Fields[5].Value);
        Assert.Contains("/orders/11111111-1111-1111-1111-111111111111?payment=success", form.Fields[7].Value);
        Assert.Equal("PQCol+dbtwGidnF59W4cFzXarv8C2DLqO3N5FSJ3ABk=", form.Fields[^1].Value);
        Assert.True(service.IsValidIpnSecret("ipn-secret-key"));
        Assert.False(service.IsValidIpnSecret("ipn-secret-kex"));
    }

    [Fact]
    public void Enabled_sepay_requires_secret_key_ipn_authentication_and_the_matching_environment_host()
    {
        var validator = new SePayOptionsValidator();
        var options = new SePayOptions
        {
            Enabled = true,
            Environment = "Production",
            MerchantId = "merchant-test",
            MerchantSecretKey = "merchant-secret",
            IpnSecretKey = "ipn-secret",
            IpnAuthenticationMode = "SecretKey",
            CheckoutInitUrl = "https://pay-sandbox.sepay.vn/v1/checkout/init",
            PublicResultBaseUrl = "https://shop.example.test"
        };

        Assert.True(validator.Validate(null, options).Failed);
        options.CheckoutInitUrl = "https://pay.sepay.vn/v1/checkout/init";
        Assert.True(validator.Validate(null, options).Succeeded);
        options.IpnAuthenticationMode = "None";
        Assert.True(validator.Validate(null, options).Failed);
    }
}
