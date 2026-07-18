namespace Ecom.Domain.Tests.Commerce;

public class MediaTests
{
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
