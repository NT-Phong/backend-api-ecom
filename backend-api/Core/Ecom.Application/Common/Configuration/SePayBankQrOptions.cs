namespace Ecom.Application.Common.Configuration;

public sealed class SePayBankQrOptions
{
    public const string SectionName = "SePayBankQr";
    public bool Enabled { get; set; }
    public string BankCode { get; set; } = string.Empty;
    public string VirtualAccountNumber { get; set; } = string.Empty;
    public string AccountHolder { get; set; } = string.Empty;
    public string PaymentCodePrefix { get; set; } = "DH";
    public string WebhookHmacSecret { get; set; } = string.Empty;
    public string WebhookUrl { get; set; } = string.Empty;
    public string QrTemplate { get; set; } = "compact";
}
