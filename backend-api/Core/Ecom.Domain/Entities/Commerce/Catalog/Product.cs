namespace Ecom.Domain.Entities;
public class Product : BaseEntity, IAggregateRoot
{
    public Guid ProducerId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? ShortDescription { get; private set; }
    public string? Description { get; private set; }
    public string? UsageInstructions { get; private set; }
    public string? StorageInstructions { get; private set; }
    public string? WarningText { get; private set; }
    public ProductStatus Status { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public DateTime? UnpublishedAt { get; private set; }
    public string? MetaTitle { get; private set; }
    public string? MetaDescription { get; private set; }

    public static Product Create(Guid producerId, string name, string slug) => new(producerId, name, slug);

    public Product(Guid producerId, string name, string slug)
    {
        ApplyDetails(producerId, name, slug, null, null, null, null, null, null, null);
        Status = ProductStatus.Draft;
    }

    public void UpdateDetails(string name, string slug, string? shortDescription, string? description, string? usageInstructions, string? storageInstructions, string? warningText, string? metaTitle, string? metaDescription)
    {
        EnsureNotDiscontinued();
        ApplyDetails(ProducerId, name, slug, shortDescription, description, usageInstructions, storageInstructions, warningText, metaTitle, metaDescription);
    }

    public ProductMedia AttachMedia(ICollection<ProductMedia> media, Guid mediaAssetId, int displayOrder,
        bool makePrimary, bool mediaIsPubliclyUsable, string? caption = null)
    {
        EnsureNotDiscontinued();
        if (mediaAssetId == Guid.Empty)
            throw new CommerceDomainException("PRODUCT_MEDIA_REQUIRED", "A media asset is required.");
        if (media.Any(x => x.MediaAssetId == mediaAssetId))
            throw new CommerceDomainException("PRODUCT_MEDIA_DUPLICATE", "The media asset is already attached to this product.");
        if (makePrimary && !mediaIsPubliclyUsable)
            throw new CommerceDomainException("PRODUCT_PRIMARY_MEDIA_INVALID", "Primary product media must be clean and public.");

        if (makePrimary)
            foreach (var item in media.Where(x => x.IsPrimary)) item.SetPrimary(false);

        var productMedia = ProductMedia.Create(Id, mediaAssetId, displayOrder, makePrimary, caption);
        media.Add(productMedia);
        return productMedia;
    }

    public void SetPrimaryMedia(ICollection<ProductMedia> media, Guid mediaAssetId, bool mediaIsPubliclyUsable)
    {
        EnsureNotDiscontinued();
        if (!mediaIsPubliclyUsable)
            throw new CommerceDomainException("PRODUCT_PRIMARY_MEDIA_INVALID", "Primary product media must be clean and public.");
        var target = media.SingleOrDefault(x => x.MediaAssetId == mediaAssetId)
            ?? throw new CommerceDomainException("PRODUCT_MEDIA_NOT_FOUND", "Product media was not found.");
        foreach (var item in media) item.SetPrimary(item == target);
    }

    public void ReorderMedia(ICollection<ProductMedia> media, Guid mediaAssetId, int displayOrder)
    {
        EnsureNotDiscontinued();
        var target = media.SingleOrDefault(x => x.MediaAssetId == mediaAssetId)
            ?? throw new CommerceDomainException("PRODUCT_MEDIA_NOT_FOUND", "Product media was not found.");
        target.Reorder(displayOrder);
    }

    public void RemoveMedia(ICollection<ProductMedia> media, Guid mediaAssetId)
    {
        EnsureNotDiscontinued();
        var target = media.SingleOrDefault(x => x.MediaAssetId == mediaAssetId)
            ?? throw new CommerceDomainException("PRODUCT_MEDIA_NOT_FOUND", "Product media was not found.");
        if (Status == ProductStatus.Published && target.IsPrimary)
            throw new CommerceDomainException("PRODUCT_PRIMARY_MEDIA_REQUIRED", "A published product cannot remove its primary media.");
        media.Remove(target);
    }

    public void SubmitForReview()
    {
        EnsureNotDiscontinued();
        if (Status != ProductStatus.Draft && Status != ProductStatus.Paused)
            throw new CommerceDomainException("PRODUCT_REVIEW_TRANSITION_INVALID", "Only draft or paused products can be submitted for review.");
        ChangeStatus(ProductStatus.Review);
    }

    public void Publish(DateTime publishedAt, bool hasPrimaryCategory, bool hasPrimaryMedia, bool hasActiveVariant, bool hasEffectivePrice)
    {
        EnsureNotDiscontinued();
        if (Status != ProductStatus.Review)
            throw new CommerceDomainException("PRODUCT_PUBLISH_TRANSITION_INVALID", "Only a product in review can be published.");
        if (!hasPrimaryCategory || !hasPrimaryMedia || !hasActiveVariant || !hasEffectivePrice)
            throw new CommerceDomainException("PRODUCT_PUBLISH_REQUIREMENTS_MISSING", "A primary category, primary media, active variant, and effective price are required.");
        if (publishedAt == default)
            throw new CommerceDomainException("PRODUCT_PUBLISHED_AT_REQUIRED", "A publication time is required.");

        ChangeStatus(ProductStatus.Published);
        PublishedAt = publishedAt;
        UnpublishedAt = null;
    }

    public void Pause(DateTime unpublishedAt)
    {
        EnsureNotDiscontinued();
        if (Status != ProductStatus.Published)
            throw new CommerceDomainException("PRODUCT_PAUSE_TRANSITION_INVALID", "Only a published product can be paused.");
        ChangeStatus(ProductStatus.Paused);
        UnpublishedAt = unpublishedAt;
    }

    public void Discontinue(DateTime discontinuedAt)
    {
        if (Status == ProductStatus.Discontinued)
            return;
        ChangeStatus(ProductStatus.Discontinued);
        UnpublishedAt = discontinuedAt;
    }

    private void ChangeStatus(ProductStatus target)
    {
        var previous = Status;
        Status = target;
        AddDomainEvent(new CommerceStateChangedEvent(nameof(Product), Id, previous.ToString(), target.ToString()));
    }

    private void EnsureNotDiscontinued()
    {
        if (Status == ProductStatus.Discontinued)
            throw new CommerceDomainException("PRODUCT_DISCONTINUED", "A discontinued product cannot be changed.");
    }

    private void ApplyDetails(Guid producerId, string name, string slug, string? shortDescription, string? description, string? usageInstructions, string? storageInstructions, string? warningText, string? metaTitle, string? metaDescription)
    {
        if (producerId == Guid.Empty)
            throw new CommerceDomainException("PRODUCT_PRODUCER_REQUIRED", "A producer is required.");
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(slug))
            throw new CommerceDomainException("PRODUCT_DETAILS_REQUIRED", "Product name and slug are required.");
        ProducerId = producerId;
        Name = name.Trim();
        Slug = slug.Trim();
        ShortDescription = shortDescription?.Trim();
        Description = description?.Trim();
        UsageInstructions = usageInstructions?.Trim();
        StorageInstructions = storageInstructions?.Trim();
        WarningText = warningText?.Trim();
        MetaTitle = metaTitle?.Trim();
        MetaDescription = metaDescription?.Trim();
    }

    private Product()
    {
    }
}
