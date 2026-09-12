namespace Ecom.Application.Features.Catalog.Common;

public sealed record CatalogSearchFacetDto(Guid Id, string Name, string? Slug, int Count);
public sealed record CatalogAvailabilityFacetDto(CatalogAvailabilityStatus Availability, int Count);
public sealed record ProductSearchResponseDto(IReadOnlyList<ProductListItemDto> Items, int TotalCount, int Page,
    int PageSize, IReadOnlyList<CatalogSearchFacetDto> Categories, IReadOnlyList<CatalogSearchFacetDto> Producers,
    IReadOnlyList<CatalogAvailabilityFacetDto> Availability);
public sealed record ProductSearchSuggestionDto(string Slug, string Name, ProductMediaDto? PrimaryMedia);
