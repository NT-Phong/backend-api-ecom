using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Catalog.Common;

public sealed record ProductManagementResult(Guid Id, string Slug, ProductStatus Status, Guid ConcurrencyStamp);
public sealed record ProductSoftDeleteResult(Guid Id, bool IsDeleted);

public sealed record ProductVariantManagementResult(Guid VariantId, Guid ProductId, Guid ConcurrencyStamp);
public sealed record VariantPriceManagementResult(Guid VariantPriceId, Guid ProductId, Guid ConcurrencyStamp);

public sealed record CatalogProductManagementDto(Guid Id, Guid ProducerId, string Name, string Slug,
    string? ShortDescription, string? Description, string? UsageInstructions, string? StorageInstructions,
    string? WarningText, string? MetaTitle, string? MetaDescription, ProductStatus Status,
    DateTime? PublishedAt, DateTime? UnpublishedAt, Guid ConcurrencyStamp,
    IReadOnlyList<CategorySummaryDto> Categories, IReadOnlyList<CatalogProductMediaDto> Media,
    IReadOnlyList<CatalogProductVariantDto> Variants, IReadOnlyList<CatalogVariantPriceDto> PricePeriods,
    string? BrandName = null);

public sealed record CatalogProductListItemDto(Guid Id, Guid ProducerId, string Name, string Slug,
    ProductStatus Status, DateTime CreatedAt, DateTime? UpdatedAt, CategorySummaryDto? PrimaryCategory,
    CatalogProductPriceDto Price, CatalogProductInventoryDto Inventory, ProductMediaDto? PrimaryMedia,
    string? BrandName);

public sealed record CatalogProductPriceDto(decimal? FromAmount, string? CurrencyCode, bool HasEffectivePrice);
public sealed record CatalogProductInventoryDto(decimal StockedQuantity, decimal ReservedQuantity,
    decimal AvailableQuantity, decimal IncomingQuantity, bool IsTracked);

public sealed record CatalogProducerPickerDto(Guid Id, string Code, string Name, PublicStatus PublicStatus,
    bool IsVerified);

public sealed record CatalogProductMediaDto(Guid MediaAssetId, string OriginalFileName, string ContentType,
    MediaType MediaType, MediaVisibility Visibility, MediaScanStatus ScanStatus, int DisplayOrder,
    bool IsPrimary, string? Caption);

public sealed record CatalogProductVariantDto(Guid Id, string Sku, string Name, VariantStatus Status,
    InventoryMode InventoryMode, bool AllowBackorder, string? Barcode, decimal? WeightGrams, int DisplayOrder);

public sealed record CatalogVariantPriceDto(Guid Id, Guid ProductVariantId, decimal Amount, string CurrencyCode,
    PriceType PriceType, int MinQuantity, DateTime EffectiveFrom, DateTime? EffectiveTo, Guid? PriceListId);
