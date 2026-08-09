namespace Ecom.Application.Common.Configuration;

public sealed class SePayOptions
{
    public const string SectionName = "SePay";

    public bool Enabled { get; set; }
    public string Environment { get; set; } = "Sandbox";
    public string MerchantId { get; set; } = string.Empty;
    public string MerchantSecretKey { get; set; } = string.Empty;
    public string IpnSecretKey { get; set; } = string.Empty;
    public string IpnAuthenticationMode { get; set; } = "SecretKey";
    public string CheckoutInitUrl { get; set; } = string.Empty;
    public string PublicResultBaseUrl { get; set; } = string.Empty;
}
