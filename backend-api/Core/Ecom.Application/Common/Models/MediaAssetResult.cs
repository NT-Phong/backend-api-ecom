using Ecom.Domain.Enums;

namespace Ecom.Application.Common.Models;

public sealed record MediaAssetResult(Guid Id, string OriginalFileName, string ContentType, long SizeBytes,
    MediaType MediaType, MediaVisibility Visibility, MediaScanStatus ScanStatus,
    MediaVisibility? IntendedVisibility = null);
