namespace Ecom.Domain.Tests.Commerce;

public class MediaTests
{
    [Fact]
    public void Pending_product_image_keeps_its_intended_public_visibility()
    {
        var media = MediaAsset.CreatePending("quarantine/image.jpg", "image.jpg", "image/jpeg", 128,
            MediaType.Image, MediaVisibility.Restricted, MediaUploadIntent.ProductImage, MediaVisibility.Public);

        Assert.Equal(MediaUploadIntent.ProductImage, media.UploadIntent);
        Assert.Equal(MediaVisibility.Public, media.TargetVisibility);
        Assert.Equal(MediaVisibility.Restricted, media.Visibility);
    }

    [Fact]
    public void Pending_media_claim_and_retry_respect_the_scan_lease()
    {
        var now = DateTime.UtcNow;
        var media = MediaAsset.CreatePending("quarantine/image.jpg", "image.jpg", "image/jpeg", 128,
            MediaType.Image, MediaVisibility.Restricted);

        Assert.True(media.TryClaimScan(now, TimeSpan.FromMinutes(5)));
        Assert.False(media.TryClaimScan(now.AddMinutes(1), TimeSpan.FromMinutes(5)));
        media.ScheduleScanRetry(now.AddMinutes(10));
        Assert.Equal(1, media.ScanAttemptCount);
        Assert.False(media.TryClaimScan(now.AddMinutes(9), TimeSpan.FromMinutes(5)));
        Assert.True(media.TryClaimScan(now.AddMinutes(10), TimeSpan.FromMinutes(5)));
    }

    [Fact]
    public void Manual_retry_resets_the_internal_retry_budget_and_failure_details()
    {
        var now = DateTime.UtcNow;
        var media = MediaAsset.CreatePending("quarantine/image.jpg", "image.jpg", "image/jpeg", 128,
            MediaType.Image, MediaVisibility.Restricted);
        media.ScheduleScanRetry(now.AddMinutes(1));
        media.ScheduleScanRetry(now.AddMinutes(2));
        media.MarkScanFailed(MediaScanFailureCodes.ScannerUnavailable,
            "The malware scanner is temporarily unavailable.", now.AddMinutes(3));

        media.RetryScan();

        Assert.Equal(MediaScanStatus.Pending, media.ScanStatus);
        Assert.Equal(0, media.ScanAttemptCount);
        Assert.Null(media.ScanFailureCode);
        Assert.Null(media.ScanFailureReason);
        Assert.Null(media.NextScanAttemptAt);
        Assert.Null(media.ScanLeaseExpiresAt);
    }

    [Fact]
    public void Terminal_scan_states_clear_a_stale_retry_schedule_and_lease()
    {
        var now = DateTime.UtcNow;
        var clean = MediaAsset.CreatePending("quarantine/clean.jpg", "clean.jpg", "image/jpeg", 128,
            MediaType.Image, MediaVisibility.Restricted);
        clean.ScheduleScanRetry(now.AddMinutes(1));
        Assert.True(clean.TryClaimScan(now.AddMinutes(1), TimeSpan.FromMinutes(5)));

        clean.MarkClean(now.AddMinutes(2));

        Assert.Null(clean.NextScanAttemptAt);
        Assert.Null(clean.ScanLeaseExpiresAt);

        var failed = MediaAsset.CreatePending("quarantine/failed.jpg", "failed.jpg", "image/jpeg", 128,
            MediaType.Image, MediaVisibility.Restricted);
        failed.ScheduleScanRetry(now.AddMinutes(1));
        failed.MarkScanFailed(MediaScanFailureCodes.ScannerUnavailable, "Scanner unavailable", now.AddMinutes(2));

        Assert.Null(failed.NextScanAttemptAt);
        Assert.Null(failed.ScanLeaseExpiresAt);

        var rejected = MediaAsset.CreatePending("quarantine/rejected.jpg", "rejected.jpg", "image/jpeg", 128,
            MediaType.Image, MediaVisibility.Restricted);
        rejected.ScheduleScanRetry(now.AddMinutes(1));
        rejected.Reject("Rejected", now.AddMinutes(2));

        Assert.Null(rejected.NextScanAttemptAt);
        Assert.Null(rejected.ScanLeaseExpiresAt);
    }

    [Fact]
    public void Media_must_be_clean_before_becoming_public()
    {
        var media = MediaAsset.CreatePending("quarantine/image.jpg", "image.jpg", "image/jpeg", 128,
            MediaType.Image, MediaVisibility.Restricted);

        var error = Assert.Throws<CommerceDomainException>(() => media.ChangeVisibility(MediaVisibility.Public));
        Assert.Equal("MEDIA_PUBLIC_REQUIRES_CLEAN_SCAN", error.Code);

        media.MarkClean(DateTime.UtcNow);
        media.ChangeVisibility(MediaVisibility.Public);

        Assert.True(media.IsPubliclyUsable);
    }

    [Fact]
    public void Rejected_media_is_terminal_and_cannot_be_public()
    {
        var media = MediaAsset.CreatePending("quarantine/file.pdf", "file.pdf", "application/pdf", 512,
            MediaType.Document, MediaVisibility.Internal);
        media.Reject("signature mismatch", DateTime.UtcNow);

        Assert.Equal(MediaScanStatus.Rejected, media.ScanStatus);
        Assert.Throws<CommerceDomainException>(() => media.MarkClean(DateTime.UtcNow));
        Assert.Throws<CommerceDomainException>(() => media.ChangeVisibility(MediaVisibility.Public));
    }

    [Fact]
    public void Product_primary_media_must_be_clean_and_public()
    {
        var product = Product.Create(Guid.NewGuid(), "Product", "product");
        var media = new List<ProductMedia>();
        var assetId = Guid.NewGuid();

        Assert.Throws<CommerceDomainException>(() =>
            product.AttachMedia(media, assetId, 0, true, false));

        product.AttachMedia(media, assetId, 0, true, true);
        Assert.True(media.Single().IsPrimary);
    }

    [Fact]
    public void Published_product_cannot_remove_primary_media()
    {
        var product = Product.Create(Guid.NewGuid(), "Product", "product");
        var media = new List<ProductMedia>();
        var assetId = Guid.NewGuid();
        product.AttachMedia(media, assetId, 0, true, true);
        product.SubmitForReview();
        product.Publish(DateTime.UtcNow, true, true, true, true);

        var error = Assert.Throws<CommerceDomainException>(() => product.RemoveMedia(media, assetId));
        Assert.Equal("PRODUCT_PRIMARY_MEDIA_REQUIRED", error.Code);
    }

    [Fact]
    public void Trade_inquiry_attachment_must_be_clean_and_non_public()
    {
        var history = new List<TradeInquiryStatusHistory>();
        var inquiry = TradeInquiry.Create("INQ-MEDIA", null, "Contact", null, null, "0900000000",
            TradeInquiryType.BulkPurchase, null, DateTime.UtcNow, history);
        var attachments = new List<InquiryAttachment>();

        Assert.Throws<CommerceDomainException>(() =>
            inquiry.AttachMedia(attachments, Guid.NewGuid(), MediaVisibility.Internal, false));
        Assert.Throws<CommerceDomainException>(() =>
            inquiry.AttachMedia(attachments, Guid.NewGuid(), MediaVisibility.Public, true));

        inquiry.AttachMedia(attachments, Guid.NewGuid(), MediaVisibility.Restricted, true);
        Assert.Single(attachments);
    }

    [Fact]
    public void Payment_proof_must_be_clean_and_restricted()
    {
        var payment = Payment.Create(Guid.NewGuid(), PaymentMethod.BankTransfer, 100m);
        var proofId = Guid.NewGuid();

        var error = Assert.Throws<CommerceDomainException>(() =>
            payment.MarkPaid(100m, "manual", "REF", DateTime.UtcNow, proofId));
        Assert.Equal("PAYMENT_PROOF_INVALID", error.Code);

        var transaction = payment.MarkPaid(100m, "manual", "REF", DateTime.UtcNow, proofId, true);
        Assert.Equal(proofId, transaction.ProofMediaAssetId);
    }

    [Fact]
    public void User_notification_follows_delivery_lifecycle()
    {
        var notification = Notification.CreateSystem("order.confirmed", "Order confirmed", "Your order is confirmed");
        var recipient = UserNotification.Create(notification.Id, Guid.NewGuid());

        Assert.Throws<CommerceDomainException>(() => recipient.MarkRead(DateTime.UtcNow));
        recipient.MarkDelivered(DateTime.UtcNow);
        recipient.MarkRead(DateTime.UtcNow);

        Assert.Equal(NotificationDeliveryStatus.Read, recipient.DeliveryStatus);
        Assert.NotNull(recipient.DeliveredAt);
        Assert.NotNull(recipient.ReadAt);
    }
}
