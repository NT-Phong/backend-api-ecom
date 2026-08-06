namespace Ecom.Application.Common.Configuration;

public sealed class MediaProcessingOptions
{
    public const string SectionName = "MediaProcessing";
    public bool Enabled { get; init; }
    public string ClamAvHost { get; init; } = "127.0.0.1";
    public int ClamAvPort { get; init; } = 3310;
    public int PollSeconds { get; init; } = 15;
}
