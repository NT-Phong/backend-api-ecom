using Ecom.Application.Common.Models;
using Ecom.Domain.Enums;

namespace Ecom.Application.Common.Interfaces;

public interface ICommerceMediaService
{
    Task<MediaAssetResult> UploadPendingAsync(Stream stream, string fileName, string contentType, long sizeBytes,
        MediaUploadIntent intent, string? altText = null, CancellationToken cancellationToken = default);
    Task<MediaAssetResult> CompleteScanAsync(Guid mediaAssetId, MediaVisibility targetVisibility, DateTime scannedAt,
        CancellationToken cancellationToken = default);
    Task RejectAsync(Guid mediaAssetId, string reason, DateTime scannedAt, CancellationToken cancellationToken = default);
    Task<Guid> AttachToProductAsync(Guid productId, Guid mediaAssetId, int displayOrder, bool makePrimary,
        string? caption = null, CancellationToken cancellationToken = default);
    Task<Guid> AttachToTradeInquiryAsync(Guid inquiryId, Guid mediaAssetId, MediaVisibility visibility,
        CancellationToken cancellationToken = default);
    Task<Guid> ConfirmBankTransferAsync(Guid paymentId, Guid proofMediaAssetId, string? reference, DateTime paidAt,
        CancellationToken cancellationToken = default);
    Task<int> CleanupPendingAsync(DateTime olderThan, int batchSize, CancellationToken cancellationToken = default);
}
