using Ecom.Domain.Enums;
using Ecom.Domain.Entities;

namespace Ecom.Application.Common.Models;

public sealed record ValidatedMediaUpload(
    string OriginalFileName,
    string ContentType,
    string SafeExtension,
    long SizeBytes,
    MediaType MediaType,
    MediaVisibility TargetVisibility);

public sealed record StoredMediaUpload(string StorageKey, ValidatedMediaUpload Metadata);

public sealed record MediaMetadataResult(Guid Id, string OriginalFileName, string ContentType, long SizeBytes,
    MediaType MediaType, MediaUploadIntent UploadIntent, MediaVisibility Visibility,
    MediaVisibility TargetVisibility, MediaScanStatus ScanStatus, string? AltText,
    string? ScanFailureCode, string? ScanFailureReason, bool CanRetryScan,
    DateTime? NextScanAttemptAt, DateTime CreatedAt);

public static class MediaMetadataResults
{
    public static MediaMetadataResult From(MediaAsset media) => new(media.Id, media.OriginalFileName,
        media.ContentType, media.SizeBytes, media.MediaType, media.UploadIntent, media.Visibility,
        media.TargetVisibility, media.ScanStatus, media.AltText, media.ScanFailureCode, media.ScanFailureReason,
        media.ScanStatus == MediaScanStatus.Failed,
        media.ScanStatus == MediaScanStatus.Pending ? media.NextScanAttemptAt : null,
        media.CreatedAt);
}
