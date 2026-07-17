namespace Ecom.Domain.Entities;
public class MediaAsset : BaseEntity
{
    public string StorageKey { get; private set; } = string.Empty;
    public string OriginalFileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long SizeBytes { get; private set; }
    public MediaType MediaType { get; private set; }
    public string? AltText { get; private set; }
    public MediaVisibility Visibility { get; private set; }
    public MediaScanStatus ScanStatus { get; private set; }

    private MediaAsset()
    {
    }
}