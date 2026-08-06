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

    public IReadOnlyList<ProductCategory> ReplaceCategories(ICollection<ProductCategory> categories,
        IReadOnlyCollection<ProductCategoryAssignment> assignments)
    {
        EnsureNotDiscontinued();
        if (assignments.Count == 0)
            throw new CommerceDomainException("PRODUCT_CATEGORY_REQUIRED", "At least one category is required.");
        if (assignments.Count(x => x.IsPrimary) != 1)
            throw new CommerceDomainException("PRODUCT_PRIMARY_CATEGORY_REQUIRED", "Exactly one primary category is required.");
        if (assignments.Any(x => x.CategoryId == Guid.Empty) || assignments.Select(x => x.CategoryId).Distinct().Count() != assignments.Count)
            throw new CommerceDomainException("PRODUCT_CATEGORY_INVALID", "Product categories must be unique and valid.");

        var replacements = assignments
            .Select(x => ProductCategory.Create(Id, x.CategoryId, x.IsPrimary))
            .ToList();
        categories.Clear();
        foreach (var category in replacements) categories.Add(category);
        return replacements;
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

    public void UpdateMedia(ICollection<ProductMedia> media, Guid mediaAssetId, int displayOrder, string? caption)
    {
        EnsureNotDiscontinued();
        var target = media.SingleOrDefault(x => x.MediaAssetId == mediaAssetId)
            ?? throw new CommerceDomainException("PRODUCT_MEDIA_NOT_FOUND", "Product media was not found.");
        target.Reorder(displayOrder);
        target.UpdateCaption(caption);
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

    public void EnsureContentCanBeChanged() => EnsureNotDiscontinued();

    public void ReturnToReviewIfPublished(DateTime changedAt)
    {
        EnsureNotDiscontinued();
        if (Status != ProductStatus.Published)
            return;

        if (changedAt == default)
            throw new CommerceDomainException("PRODUCT_CHANGE_TIME_REQUIRED", "A content change time is required.");

        ChangeStatus(ProductStatus.Review);
        UnpublishedAt = changedAt;
    }

    public ProductOption AddOption(ICollection<ProductOption> options, string code, string name, int displayOrder)
    {
        EnsureNotDiscontinued();
        if (options.Any(x => string.Equals(x.Code, code.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new CommerceDomainException("PRODUCT_OPTION_DUPLICATE", "Product option code must be unique.");
        var option = ProductOption.Create(Id, code, name, displayOrder);
        options.Add(option);
        return option;
    }

    public void UpdateOption(ICollection<ProductOption> options, Guid optionId, string name, int displayOrder)
    {
        EnsureNotDiscontinued();
        var option = options.SingleOrDefault(x => x.Id == optionId)
            ?? throw new CommerceDomainException("PRODUCT_OPTION_NOT_FOUND", "Product option was not found.");
        option.Update(name, displayOrder);
    }

    public IReadOnlyList<ProductOptionValue> RemoveOption(ICollection<ProductOption> options,
        ICollection<ProductOptionValue> values, IReadOnlyCollection<ProductVariantOptionValue> mappings, Guid optionId)
    {
        EnsureNotDiscontinued();
        var option = options.SingleOrDefault(x => x.Id == optionId)
            ?? throw new CommerceDomainException("PRODUCT_OPTION_NOT_FOUND", "Product option was not found.");
        var optionValues = values.Where(x => x.ProductOptionId == optionId).ToList();
        if (mappings.Any(x => optionValues.Select(value => value.Id).Contains(x.ProductOptionValueId)))
            throw new CommerceDomainException("PRODUCT_OPTION_IN_USE", "An option used by a variant cannot be removed.");
        foreach (var value in optionValues) values.Remove(value);
        options.Remove(option);
        return optionValues;
    }

    public ProductOptionValue AddOptionValue(ICollection<ProductOption> options, ICollection<ProductOptionValue> values,
        Guid optionId, string value, int displayOrder)
    {
        EnsureNotDiscontinued();
        var option = options.SingleOrDefault(x => x.Id == optionId)
            ?? throw new CommerceDomainException("PRODUCT_OPTION_NOT_FOUND", "Product option was not found.");
        if (values.Any(x => x.ProductOptionId == option.Id && string.Equals(x.Value, value.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new CommerceDomainException("PRODUCT_OPTION_VALUE_DUPLICATE", "Product option value must be unique.");
        var result = ProductOptionValue.Create(option.Id, value, displayOrder);
        values.Add(result);
        return result;
    }

    public void UpdateOptionValue(ICollection<ProductOption> options, ICollection<ProductOptionValue> values,
        Guid optionId, Guid valueId, string value, int displayOrder)
    {
        EnsureNotDiscontinued();
        if (options.All(x => x.Id != optionId)) throw new CommerceDomainException("PRODUCT_OPTION_NOT_FOUND", "Product option was not found.");
        var target = values.SingleOrDefault(x => x.Id == valueId && x.ProductOptionId == optionId)
            ?? throw new CommerceDomainException("PRODUCT_OPTION_VALUE_NOT_FOUND", "Product option value was not found.");
        if (values.Any(x => x.Id != target.Id && x.ProductOptionId == optionId && string.Equals(x.Value, value.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new CommerceDomainException("PRODUCT_OPTION_VALUE_DUPLICATE", "Product option value must be unique.");
        target.Update(value, displayOrder);
    }

    public ProductOptionValue RemoveOptionValue(ICollection<ProductOption> options, ICollection<ProductOptionValue> values,
        IReadOnlyCollection<ProductVariantOptionValue> mappings, Guid optionId, Guid valueId)
    {
        EnsureNotDiscontinued();
        if (options.All(x => x.Id != optionId))
            throw new CommerceDomainException("PRODUCT_OPTION_NOT_FOUND", "Product option was not found.");
        var target = values.SingleOrDefault(x => x.Id == valueId && x.ProductOptionId == optionId)
            ?? throw new CommerceDomainException("PRODUCT_OPTION_VALUE_NOT_FOUND", "Product option value was not found.");
        if (mappings.Any(x => x.ProductOptionValueId == valueId))
            throw new CommerceDomainException("PRODUCT_OPTION_VALUE_IN_USE", "An option value used by a variant cannot be removed.");
        values.Remove(target);
        return target;
    }

    public IReadOnlyList<ProductVariantOptionValue> ReplaceVariantOptionValues(ICollection<ProductVariant> variants,
        ICollection<ProductOption> options, ICollection<ProductOptionValue> values, ICollection<ProductVariantOptionValue> mappings,
        Guid variantId, IReadOnlyCollection<Guid> valueIds)
    {
        EnsureNotDiscontinued();
        if (variants.All(x => x.Id != variantId)) throw new CommerceDomainException("PRODUCT_VARIANT_NOT_FOUND", "Product variant was not found.");
        if (valueIds.Count != valueIds.Distinct().Count()) throw new CommerceDomainException("VARIANT_OPTION_VALUE_DUPLICATE", "Variant option values must be unique.");
        var selected = values.Where(x => valueIds.Contains(x.Id)).ToList();
        if (selected.Count != valueIds.Count || selected.Any(x => options.All(o => o.Id != x.ProductOptionId)))
            throw new CommerceDomainException("VARIANT_OPTION_VALUE_INVALID", "Variant option values do not belong to this product.");
        if (selected.Select(x => x.ProductOptionId).Distinct().Count() != selected.Count)
            throw new CommerceDomainException("VARIANT_OPTION_VALUE_CONFLICT", "A variant can select only one value per option.");
        var old = mappings.Where(x => x.ProductVariantId == variantId).ToList();
        foreach (var item in old) mappings.Remove(item);
        var replacements = selected.Select(x => ProductVariantOptionValue.Create(variantId, x.Id)).ToList();
        foreach (var item in replacements) mappings.Add(item);
        return replacements;
    }

    public Guid RenewConcurrencyStamp()
    {
        ConcurrencyStamp = Guid.NewGuid();
        return ConcurrencyStamp;
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
