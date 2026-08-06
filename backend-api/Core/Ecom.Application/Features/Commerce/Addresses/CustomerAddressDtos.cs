using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Addresses;

public sealed record CustomerAddressDto(Guid Id, Guid? AdministrativeAreaId, string RecipientName, string PhoneNumber,
    string AddressLine, string? PostalCode, string? Label, bool IsDefault);
internal static class CustomerAddressMapper
{
    internal static CustomerAddressDto Map(CustomerAddress x) => new(x.Id, x.AdministrativeAreaId, x.RecipientName,
        x.PhoneNumber, x.AddressLine, x.PostalCode, x.Label, x.IsDefault);
}
