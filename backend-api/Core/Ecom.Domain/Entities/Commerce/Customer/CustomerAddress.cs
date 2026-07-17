namespace Ecom.Domain.Entities;
public class CustomerAddress : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid? AdministrativeAreaId { get; private set; }
    public string RecipientName { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public string AddressLine { get; private set; } = string.Empty;
    public string? PostalCode { get; private set; }
    public decimal? Latitude { get; private set; }
    public decimal? Longitude { get; private set; }
    public string? Label { get; private set; }
    public bool IsDefault { get; private set; }

    private CustomerAddress()
    {
    }
}