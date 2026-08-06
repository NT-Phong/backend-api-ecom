namespace Ecom.Application.Features.Catalog.Options;

public sealed record ProductOptionDto(
    Guid Id,
    string Code,
    string Name,
    int DisplayOrder,
    IReadOnlyList<ProductOptionValueDto> Values);

public sealed record ProductOptionValueDto(Guid Id, string Value, int DisplayOrder);
