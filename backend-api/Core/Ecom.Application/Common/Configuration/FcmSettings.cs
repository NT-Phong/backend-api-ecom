namespace Ecom.Application.Common.Configuration;

public class FcmSettings
{
    public const string SectionName = "Fcm";
    public string CredentialsJson { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
}

