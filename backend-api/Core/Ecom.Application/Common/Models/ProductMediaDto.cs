namespace Ecom.Application.Common.Models;

public sealed record ProductMediaDto(Guid MediaAssetId, string Url, string ContentType, string? AltText,
    string? Caption, int DisplayOrder, bool IsPrimary);
