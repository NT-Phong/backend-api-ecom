namespace Ecom.Application.Common.Configuration;

public static class CommerceRateLimitPolicyNames
{
    public const string CartMutation = "commerce-cart-mutation";
    public const string CheckoutPreview = "commerce-checkout-preview";
    public const string OrderCreate = "commerce-order-create";
    public const string ManagementMutation = "commerce-management-mutation";
}

public sealed class CommerceRateLimitOptions
{
    public const string SectionName = "CommerceRateLimits";

    public RateLimitRule CartMutation { get; set; } = new(60, 60);
    public RateLimitRule CheckoutPreview { get; set; } = new(30, 60);
    public RateLimitRule OrderCreate { get; set; } = new(10, 60);
    public RateLimitRule ManagementMutation { get; set; } = new(60, 60);
}
