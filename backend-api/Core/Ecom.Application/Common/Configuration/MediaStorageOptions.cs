namespace Ecom.Application.Common.Configuration;

public sealed class MediaStorageOptions
{
    public const string SectionName = "MediaStorage";
    public string Provider { get; init; } = "Local";
    public string? AccountUrl { get; init; }
    public string? ConnectionString { get; init; }
    public string QuarantineContainer { get; init; } = "ecom-media-quarantine";
    public string PublicContainer { get; init; } = "ecom-media-public";
    public string PrivateContainer { get; init; } = "ecom-media-private";
    public string? PublicBaseUrl { get; init; }
}
