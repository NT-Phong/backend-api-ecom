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

    public static CustomerAddress Create(Guid userId, Guid? administrativeAreaId, string recipientName,
        string phoneNumber, string addressLine, string? postalCode, string? label, bool isDefault)
    {
        if (userId == Guid.Empty || string.IsNullOrWhiteSpace(recipientName) ||
            string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(addressLine))
            throw new CommerceDomainException("CUSTOMER_ADDRESS_REQUIRED", "Recipient, phone, and address are required.");

        return new CustomerAddress
        {
            UserId = userId,
            AdministrativeAreaId = administrativeAreaId,
            RecipientName = recipientName.Trim(),
            PhoneNumber = phoneNumber.Trim(),
            AddressLine = addressLine.Trim(),
            PostalCode = postalCode?.Trim(),
            Label = label?.Trim(),
            IsDefault = isDefault
        };
    }

    public void Update(Guid? administrativeAreaId, string recipientName, string phoneNumber, string addressLine,
        string? postalCode, string? label)
    {
        if (string.IsNullOrWhiteSpace(recipientName) || string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(addressLine))
            throw new CommerceDomainException("CUSTOMER_ADDRESS_REQUIRED", "Recipient, phone, and address are required.");
        AdministrativeAreaId = administrativeAreaId;
        RecipientName = recipientName.Trim();
        PhoneNumber = phoneNumber.Trim();
        AddressLine = addressLine.Trim();
        PostalCode = postalCode?.Trim();
        Label = label?.Trim();
    }

    public void SetDefault(bool isDefault) => IsDefault = isDefault;

    public void Deactivate() => IsDeleted = true;

    private CustomerAddress()
    {
    }
}
