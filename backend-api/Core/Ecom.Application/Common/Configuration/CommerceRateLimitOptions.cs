namespace Ecom.Application.Common.Configuration;

public static class CommerceRateLimitPolicyNames
{
    public const string CartMutation = "commerce-cart-mutation";
    public const string CheckoutPreview = "commerce-checkout-preview";
    public const string OrderCreate = "commerce-order-create";
    public const string PaymentCheckout = "commerce-payment-checkout";
    public const string PaymentIpn = "commerce-payment-ipn";
    public const string PaymentBankWebhook = "commerce-payment-bank-webhook";
    public const string ManagementMutation = "commerce-management-mutation";
}

public sealed class CommerceRateLimitOptions
{
    public const string SectionName = "CommerceRateLimits";

    public RateLimitRule CartMutation { get; set; } = new(60, 60);
    public RateLimitRule CheckoutPreview { get; set; } = new(30, 60);
    public RateLimitRule OrderCreate { get; set; } = new(10, 60);
    public RateLimitRule PaymentCheckout { get; set; } = new(20, 60);
    public RateLimitRule PaymentIpn { get; set; } = new(120, 60);
    public RateLimitRule PaymentBankWebhook { get; set; } = new(120, 60);
    public RateLimitRule ManagementMutation { get; set; } = new(60, 60);
}
