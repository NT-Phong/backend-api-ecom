namespace Ecom.Application.Features.Catalog.Common;

public sealed record ProductListItemDto(Guid Id, string Slug, string Name, string? ShortDescription,
    ProducerSummaryDto Producer, CategorySummaryDto PrimaryCategory, ProductMediaDto? PrimaryMedia,
    decimal FromPrice, string CurrencyCode, DateTime PublishedAt);

public sealed record ProductDetailDto(Guid Id, string Slug, string Name, string? ShortDescription,
    string? Description, string? UsageInstructions, string? StorageInstructions, string? WarningText,
    string? MetaTitle, string? MetaDescription, ProducerSummaryDto Producer,
    IReadOnlyList<CategorySummaryDto> Categories, IReadOnlyList<ProductMediaDto> Media,
    IReadOnlyList<ProductVariantDto> Variants, DateTime PublishedAt);

public sealed record ProducerSummaryDto(Guid Id, string Code, string Name, string? Description, string? WebsiteUrl);
public sealed record CategorySummaryDto(Guid Id, string Name, string Slug, bool IsPrimary, int DisplayOrder);
public sealed record ProductVariantDto(Guid Id, string Sku, string Name, decimal Price, string CurrencyCode,
    PriceType PriceType, decimal? WeightGrams, IReadOnlyList<VariantOptionValueDto> Options);
public sealed record VariantOptionValueDto(Guid OptionId, string Code, string Name, Guid ValueId, string Value);
public sealed record PublicCategoryDto(Guid Id, Guid? ParentId, string Name, string Slug, string? Description, int DisplayOrder);
