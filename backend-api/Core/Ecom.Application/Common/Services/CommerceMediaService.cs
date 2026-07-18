using Ecom.Domain.Entities;

namespace Ecom.Application.Common.Services;

public sealed class CommerceMediaService(
    IUnitOfWork unitOfWork,
    IMediaFileService mediaFiles,
    ILogger<CommerceMediaService> logger) : ICommerceMediaService
{
    public async Task<MediaAssetResult> UploadPendingAsync(Stream stream, string fileName, string contentType,
        long sizeBytes, MediaUploadIntent intent, string? altText = null, CancellationToken cancellationToken = default)
    {
        StoredMediaUpload? stored = null;
        try
        {
            stored = await mediaFiles.StorePendingAsync(stream, fileName, contentType, sizeBytes, intent, cancellationToken);
            var media = MediaAsset.CreatePending(stored.StorageKey, stored.Metadata.OriginalFileName,
                stored.Metadata.ContentType, stored.Metadata.SizeBytes, stored.Metadata.MediaType,
                MediaVisibility.Restricted, altText);
            await unitOfWork.Repository<MediaAsset>().InsertAsync(media, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return ToResult(media, stored.Metadata.TargetVisibility);
        }
        catch
        {
            if (stored is not null) await TryDeleteAsync(stored.StorageKey, cancellationToken);
            throw;
        }
    }

    public async Task<MediaAssetResult> CompleteScanAsync(Guid mediaAssetId, MediaVisibility targetVisibility,
        DateTime scannedAt, CancellationToken cancellationToken = default)
    {
        var media = await GetMediaAsync(mediaAssetId);
        var quarantineKey = media.StorageKey;
        string? promotedKey = null;
        try
        {
            promotedKey = await mediaFiles.PromoteAsync(quarantineKey, targetVisibility, cancellationToken);
            media.MarkClean(promotedKey, targetVisibility, scannedAt);
            await unitOfWork.Repository<MediaAsset>().UpdateAsync(media, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            if (promotedKey is not null) await TryDeleteAsync(promotedKey, cancellationToken);
            throw;
        }

        await TryDeleteAsync(quarantineKey, cancellationToken);
        return ToResult(media);
    }

    public async Task RejectAsync(Guid mediaAssetId, string reason, DateTime scannedAt,
        CancellationToken cancellationToken = default)
    {
        var media = await GetMediaAsync(mediaAssetId);
        media.Reject(reason, scannedAt);
        await unitOfWork.Repository<MediaAsset>().UpdateAsync(media, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<Guid> AttachToProductAsync(Guid productId, Guid mediaAssetId, int displayOrder,
        bool makePrimary, string? caption = null, CancellationToken cancellationToken = default)
    {
        var product = await unitOfWork.Repository<Product>().FindByIdAsync(productId)
            ?? throw new CommerceDomainException("PRODUCT_NOT_FOUND", "Product was not found.");
        var asset = await GetMediaAsync(mediaAssetId);
        var links = await unitOfWork.Repository<ProductMedia>().FindAsync([x => x.ProductId == productId]);
        var existing = links.ToList();
        var link = product.AttachMedia(links, mediaAssetId, displayOrder, makePrimary, asset.IsPubliclyUsable, caption);
        if (makePrimary && existing.Count > 0)
            await unitOfWork.Repository<ProductMedia>().UpdateRangeAsync(existing, cancellationToken);
        await unitOfWork.Repository<ProductMedia>().InsertAsync(link, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return link.Id;
    }

    public async Task<Guid> AttachToTradeInquiryAsync(Guid inquiryId, Guid mediaAssetId, MediaVisibility visibility,
        CancellationToken cancellationToken = default)
    {
        var inquiry = await unitOfWork.Repository<TradeInquiry>().FindByIdAsync(inquiryId)
            ?? throw new CommerceDomainException("TRADE_INQUIRY_NOT_FOUND", "Trade inquiry was not found.");
        var asset = await GetMediaAsync(mediaAssetId);
        var attachments = await unitOfWork.Repository<InquiryAttachment>().FindAsync([x => x.TradeInquiryId == inquiryId]);
        var attachment = inquiry.AttachMedia(attachments, mediaAssetId, visibility,
            asset.ScanStatus == MediaScanStatus.Clean);
        await unitOfWork.Repository<InquiryAttachment>().InsertAsync(attachment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return attachment.Id;
    }

    public async Task<Guid> ConfirmBankTransferAsync(Guid paymentId, Guid proofMediaAssetId, string? reference,
        DateTime paidAt, CancellationToken cancellationToken = default)
    {
        var payment = await unitOfWork.Repository<Payment>().FindByIdAsync(paymentId)
            ?? throw new CommerceDomainException("PAYMENT_NOT_FOUND", "Payment was not found.");
        if (payment.Method != PaymentMethod.BankTransfer)
            throw new CommerceDomainException("PAYMENT_METHOD_INVALID", "Only bank transfers accept manual proof confirmation.");
        var proof = await GetMediaAsync(proofMediaAssetId);
        var proofIsValid = proof.ScanStatus == MediaScanStatus.Clean && proof.Visibility == MediaVisibility.Restricted;
        var transaction = payment.MarkPaid(payment.Amount, "manual", reference, paidAt,
            proofMediaAssetId, proofIsValid);
        await unitOfWork.Repository<Payment>().UpdateAsync(payment, cancellationToken);
        await unitOfWork.Repository<PaymentTransaction>().InsertAsync(transaction, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return transaction.Id;
    }

    public async Task<int> CleanupPendingAsync(DateTime olderThan, int batchSize,
        CancellationToken cancellationToken = default)
    {
        if (batchSize is < 1 or > 500) throw new ArgumentOutOfRangeException(nameof(batchSize));
        var stale = await unitOfWork.Repository<MediaAsset>().FindAsync(
            [x => x.ScanStatus != MediaScanStatus.Clean && x.CreatedAt < olderThan],
            orderBy: "CreatedAt", limit: batchSize);
        foreach (var media in stale)
        {
            await TryDeleteAsync(media.StorageKey, cancellationToken);
            await unitOfWork.Repository<MediaAsset>().DeleteAsync(media, cancellationToken);
        }
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return stale.Count;
    }

    private async Task<MediaAsset> GetMediaAsync(Guid id) =>
        await unitOfWork.Repository<MediaAsset>().FindByIdAsync(id)
        ?? throw new CommerceDomainException("MEDIA_NOT_FOUND", "Media asset was not found.");

    private async Task TryDeleteAsync(string storageKey, CancellationToken cancellationToken)
    {
        try { await mediaFiles.DeleteAsync(storageKey, cancellationToken); }
        catch (Exception ex) { logger.LogWarning(ex, "Unable to delete media storage key {StorageKey}", storageKey); }
    }

    private static MediaAssetResult ToResult(MediaAsset media, MediaVisibility? intendedVisibility = null) =>
        new(media.Id, media.OriginalFileName, media.ContentType, media.SizeBytes, media.MediaType,
            media.Visibility, media.ScanStatus, intendedVisibility);
}
