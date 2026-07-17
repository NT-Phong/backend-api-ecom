namespace Ecom.Domain.Entities;
public class Product : BaseEntity
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

    public Product(Guid producerId, string name, string slug)
    {
        ApplyDetails(producerId, name, slug, null, null, null, null, null, null, null);
        Status = ProductStatus.Draft;
    }

    public void UpdateDetails(string name, string slug, string? shortDescription, string? description, string? usageInstructions, string? storageInstructions, string? warningText, string? metaTitle, string? metaDescription) => ApplyDetails(ProducerId, name, slug, shortDescription, description, usageInstructions, storageInstructions, warningText, metaTitle, metaDescription);
    public void Publish(DateTime publishedAt)
    {
        if (Status == ProductStatus.Discontinued)
            throw new InvalidOperationException("A discontinued product cannot be published.");
        Status = ProductStatus.Published;
        PublishedAt = publishedAt;
        UnpublishedAt = null;
    }

    public void Pause(DateTime unpublishedAt)
    {
        Status = ProductStatus.Paused;
        UnpublishedAt = unpublishedAt;
    }

    private void ApplyDetails(Guid producerId, string name, string slug, string? shortDescription, string? description, string? usageInstructions, string? storageInstructions, string? warningText, string? metaTitle, string? metaDescription)
    {
        if (producerId == Guid.Empty)
            throw new ArgumentException("Producer is required.", nameof(producerId));
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Product name and slug are required.");
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