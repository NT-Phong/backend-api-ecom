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
    public DateTime? ScannedAt { get; private set; }
    public string? ScanFailureReason { get; private set; }

    public bool IsPubliclyUsable => Visibility == MediaVisibility.Public && ScanStatus == MediaScanStatus.Clean;

    public static MediaAsset CreatePending(
        string storageKey,
        string originalFileName,
        string contentType,
        long sizeBytes,
        MediaType mediaType,
        MediaVisibility visibility,
        string? altText = null)
    {
        if (string.IsNullOrWhiteSpace(storageKey) || string.IsNullOrWhiteSpace(originalFileName) ||
            string.IsNullOrWhiteSpace(contentType))
            throw new CommerceDomainException("MEDIA_DETAILS_REQUIRED", "Storage key, file name, and content type are required.");
        if (sizeBytes <= 0)
            throw new CommerceDomainException("MEDIA_SIZE_INVALID", "Media size must be greater than zero.");

        return new MediaAsset
        {
            StorageKey = storageKey.Trim(),
            OriginalFileName = Path.GetFileName(originalFileName.Trim()),
            ContentType = contentType.Trim().ToLowerInvariant(),
            SizeBytes = sizeBytes,
            MediaType = mediaType,
            AltText = altText?.Trim(),
            Visibility = visibility,
            ScanStatus = MediaScanStatus.Pending
        };
    }

    public void MarkClean(DateTime scannedAt)
    {
        EnsurePendingScan();
        EnsureOccurredAt(scannedAt);
        ScanStatus = MediaScanStatus.Clean;
        ScannedAt = scannedAt;
        ScanFailureReason = null;
    }

    public void MarkClean(string promotedStorageKey, MediaVisibility visibility, DateTime scannedAt)
    {
        if (string.IsNullOrWhiteSpace(promotedStorageKey))
            throw new CommerceDomainException("MEDIA_STORAGE_KEY_REQUIRED", "A promoted storage key is required.");
        MarkClean(scannedAt);
        StorageKey = promotedStorageKey.Trim();
        ChangeVisibility(visibility);
    }

    public void Reject(string reason, DateTime scannedAt) => CompleteFailedScan(MediaScanStatus.Rejected, reason, scannedAt);

    public void MarkScanFailed(string reason, DateTime scannedAt) => CompleteFailedScan(MediaScanStatus.Failed, reason, scannedAt);

    public void RetryScan()
    {
        if (ScanStatus != MediaScanStatus.Failed)
            throw new CommerceDomainException("MEDIA_SCAN_RETRY_INVALID", "Only a failed media scan can be retried.");
        ScanStatus = MediaScanStatus.Pending;
        ScannedAt = null;
        ScanFailureReason = null;
    }

    public void ChangeVisibility(MediaVisibility visibility)
    {
        if (visibility == MediaVisibility.Public && ScanStatus != MediaScanStatus.Clean)
            throw new CommerceDomainException("MEDIA_PUBLIC_REQUIRES_CLEAN_SCAN", "Only clean media can be public.");
        Visibility = visibility;
    }

    public void UpdateAltText(string? altText) => AltText = altText?.Trim();

    private void CompleteFailedScan(MediaScanStatus target, string reason, DateTime scannedAt)
    {
        EnsurePendingScan();
        EnsureOccurredAt(scannedAt);
        if (string.IsNullOrWhiteSpace(reason))
            throw new CommerceDomainException("MEDIA_SCAN_REASON_REQUIRED", "A scan failure reason is required.");
        ScanStatus = target;
        ScannedAt = scannedAt;
        ScanFailureReason = reason.Trim();
        if (Visibility == MediaVisibility.Public)
            Visibility = MediaVisibility.Restricted;
    }

    private void EnsurePendingScan()
    {
        if (ScanStatus != MediaScanStatus.Pending)
            throw new CommerceDomainException("MEDIA_SCAN_STATE_INVALID", "Only pending media can complete a scan.");
    }

    private static void EnsureOccurredAt(DateTime occurredAt)
    {
        if (occurredAt == default)
            throw new CommerceDomainException("MEDIA_SCAN_TIME_REQUIRED", "A scan time is required.");
    }

    private MediaAsset()
    {
    }
}
