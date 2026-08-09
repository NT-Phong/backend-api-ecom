namespace Ecom.Application.Common.Configuration;

public sealed class MediaProcessingOptions
{
    public const string SectionName = "MediaProcessing";
    public bool Enabled { get; init; }
    /// <summary>
    /// Demo-only opt-in: uploads ProductImage objects straight to public storage and marks them clean.
    /// This deliberately bypasses ClamAV and must not be enabled for untrusted production uploads.
    /// </summary>
    public bool DirectPublicUploadEnabled { get; init; }
    public string ClamAvHost { get; init; } = "127.0.0.1";
    public int ClamAvPort { get; init; } = 3310;
    public int PollSeconds { get; init; } = 15;
}
