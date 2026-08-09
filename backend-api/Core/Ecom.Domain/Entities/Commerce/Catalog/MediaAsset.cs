namespace Ecom.Domain.Entities;
public class MediaAsset : BaseEntity
{
    public string StorageKey { get; private set; } = string.Empty;
    public string OriginalFileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long SizeBytes { get; private set; }
    public MediaUploadIntent UploadIntent { get; private set; }
    public MediaVisibility TargetVisibility { get; private set; }
    public MediaType MediaType { get; private set; }
    public string? AltText { get; private set; }
    public MediaVisibility Visibility { get; private set; }
    public MediaScanStatus ScanStatus { get; private set; }
    public DateTime? ScannedAt { get; private set; }
    public string? ScanFailureCode { get; private set; }
    public string? ScanFailureReason { get; private set; }
    public string? ThumbnailStorageKey { get; private set; }
    public string? Sha256 { get; private set; }
    public int ScanAttemptCount { get; private set; }
    public DateTime? NextScanAttemptAt { get; private set; }
    public DateTime? ScanLeaseExpiresAt { get; private set; }

    public bool IsPubliclyUsable => Visibility == MediaVisibility.Public && ScanStatus == MediaScanStatus.Clean;

    public static MediaAsset CreatePending(
        string storageKey,
        string originalFileName,
        string contentType,
        long sizeBytes,
        MediaType mediaType,
        MediaVisibility visibility,
        MediaUploadIntent uploadIntent = MediaUploadIntent.ProductImage,
        MediaVisibility? targetVisibility = null,
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
            UploadIntent = uploadIntent,
            TargetVisibility = targetVisibility ?? visibility,
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
        ScanFailureCode = null;
        ScanFailureReason = null;
        NextScanAttemptAt = null;
        ScanLeaseExpiresAt = null;
    }

    public void MarkClean(string promotedStorageKey, MediaVisibility visibility, DateTime scannedAt)
    {
        if (string.IsNullOrWhiteSpace(promotedStorageKey))
            throw new CommerceDomainException("MEDIA_STORAGE_KEY_REQUIRED", "A promoted storage key is required.");
        MarkClean(scannedAt);
        StorageKey = promotedStorageKey.Trim();
        ChangeVisibility(visibility);
    }

    public void SetThumbnailStorageKey(string? thumbnailStorageKey)
    {
        ThumbnailStorageKey = string.IsNullOrWhiteSpace(thumbnailStorageKey) ? null : thumbnailStorageKey.Trim();
    }

    public void SetSha256(string sha256)
    {
        if (string.IsNullOrWhiteSpace(sha256) || sha256.Trim().Length != 64 || !sha256.Trim().All(Uri.IsHexDigit))
            throw new CommerceDomainException("MEDIA_HASH_INVALID", "A SHA-256 hash is required.");
        Sha256 = sha256.Trim().ToLowerInvariant();
    }

    public bool TryClaimScan(DateTime now, TimeSpan leaseDuration)
    {
        if (ScanStatus != MediaScanStatus.Pending || now == default || leaseDuration <= TimeSpan.Zero ||
            (NextScanAttemptAt is not null && NextScanAttemptAt > now) ||
            (ScanLeaseExpiresAt is not null && ScanLeaseExpiresAt > now)) return false;
        ScanLeaseExpiresAt = now.Add(leaseDuration);
        return true;
    }

    public void ScheduleScanRetry(DateTime nextAttemptAt)
    {
        EnsurePendingScan();
        EnsureOccurredAt(nextAttemptAt);
        ScanAttemptCount++;
        NextScanAttemptAt = nextAttemptAt;
        ScanLeaseExpiresAt = null;
    }

    public void Reject(string reason, DateTime scannedAt) =>
        CompleteFailedScan(MediaScanStatus.Rejected, MediaScanFailureCodes.FileRejected, reason, scannedAt);

    public void MarkScanFailed(string failureCode, string reason, DateTime scannedAt) =>
        CompleteFailedScan(MediaScanStatus.Failed, failureCode, reason, scannedAt);

    public void RetryScan()
    {
        if (ScanStatus != MediaScanStatus.Failed)
            throw new CommerceDomainException("MEDIA_SCAN_RETRY_INVALID", "Only a failed media scan can be retried.");
        ScanStatus = MediaScanStatus.Pending;
        ScannedAt = null;
        ScanFailureCode = null;
        ScanFailureReason = null;
        ScanAttemptCount = 0;
        NextScanAttemptAt = null;
        ScanLeaseExpiresAt = null;
    }

    public void ChangeVisibility(MediaVisibility visibility)
    {
        if (visibility == MediaVisibility.Public && ScanStatus != MediaScanStatus.Clean)
            throw new CommerceDomainException("MEDIA_PUBLIC_REQUIRES_CLEAN_SCAN", "Only clean media can be public.");
        Visibility = visibility;
    }

    public void UpdateAltText(string? altText) => AltText = altText?.Trim();

    private void CompleteFailedScan(MediaScanStatus target, string failureCode, string reason, DateTime scannedAt)
    {
        EnsurePendingScan();
        EnsureOccurredAt(scannedAt);
        if (string.IsNullOrWhiteSpace(failureCode))
            throw new CommerceDomainException("MEDIA_SCAN_FAILURE_CODE_REQUIRED", "A media scan failure code is required.");
        if (string.IsNullOrWhiteSpace(reason))
            throw new CommerceDomainException("MEDIA_SCAN_REASON_REQUIRED", "A scan failure reason is required.");
        ScanStatus = target;
        ScannedAt = scannedAt;
        ScanFailureCode = failureCode.Trim();
        ScanFailureReason = reason.Trim();
        NextScanAttemptAt = null;
        ScanLeaseExpiresAt = null;
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

public static class MediaScanFailureCodes
{
    public const string FileRejected = "FILE_REJECTED";
    public const string ScannerUnavailable = "SCANNER_UNAVAILABLE";
    public const string ThumbnailGenerationFailed = "THUMBNAIL_GENERATION_FAILED";
    public const string StorageProcessingFailed = "STORAGE_PROCESSING_FAILED";
    public const string ProcessingFailed = "MEDIA_PROCESSING_FAILED";
}
