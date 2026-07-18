using Ecom.Domain.Enums;

namespace Ecom.Application.Common.Models;

public enum MediaUploadIntent
{
    ProductImage,
    TradeInquiryAttachment,
    BankTransferProof
}

public sealed record ValidatedMediaUpload(
    string OriginalFileName,
    string ContentType,
    string SafeExtension,
    long SizeBytes,
    MediaType MediaType,
    MediaVisibility TargetVisibility);

public sealed record StoredMediaUpload(string StorageKey, ValidatedMediaUpload Metadata);
