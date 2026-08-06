using Ecom.Domain.Enums;

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
    string? ScanFailureReason, DateTime CreatedAt);
