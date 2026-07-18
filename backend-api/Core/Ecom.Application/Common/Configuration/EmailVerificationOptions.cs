namespace Ecom.Application.Common.Configuration;
public sealed class EmailVerificationOptions
{
 public const string SectionName="EmailVerification";
 public bool Enabled { get; set; }
 public string CallbackBaseUrl { get; set; }=string.Empty;
}
