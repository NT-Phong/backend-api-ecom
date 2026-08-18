namespace Ecom.Application.Features.Commerce.Producers;

public sealed record ProducerListItemDto(Guid Id, string Code, string Name, string? LegalName,
    PublicStatus PublicStatus, bool IsVerified, DateTime? VerifiedAt, int FacilityCount, int ProductCount,
    Guid ConcurrencyStamp, DateTime CreatedAt, DateTime? UpdatedAt);

public sealed record ProducerContactDto(Guid Id, ContactType ContactType, string ContactValue, string? ContactName,
    bool IsPublic, int DisplayOrder);

public sealed record ProductionFacilityDto(Guid Id, Guid? AdministrativeAreaId, string Name, string? AddressLine,
    decimal? Latitude, decimal? Longitude, PublicStatus PublicStatus, string? Description);

public sealed record ProducerManagementDto(Guid Id, string Code, string Name, string? LegalName, string? Description,
    string? WebsiteUrl, PublicStatus PublicStatus, bool IsVerified, DateTime? VerifiedAt, Guid? VerifiedByUserId,
    Guid ConcurrencyStamp, IReadOnlyList<ProducerContactDto> Contacts, IReadOnlyList<ProductionFacilityDto> Facilities);

public sealed record ProducerManagementResult(Guid Id, PublicStatus PublicStatus, bool IsVerified, Guid ConcurrencyStamp);
