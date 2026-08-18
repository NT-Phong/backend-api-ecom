namespace Ecom.Domain.Entities;
public class ProducerContact : BaseEntity
{
    public Guid ProducerId { get; private set; }
    public ContactType ContactType { get; private set; }
    public string ContactValue { get; private set; } = string.Empty;
    public string? ContactName { get; private set; }
    public bool IsPublic { get; private set; }
    public int DisplayOrder { get; private set; }

    public static ProducerContact Create(Guid producerId, ContactType contactType, string contactValue,
        string? contactName, bool isPublic, int displayOrder)
    {
        if (producerId == Guid.Empty || string.IsNullOrWhiteSpace(contactValue) || displayOrder < 0)
            throw new CommerceDomainException("PRODUCER_CONTACT_INVALID", "Producer contact details are invalid.");
        return new ProducerContact
        {
            ProducerId = producerId,
            ContactType = contactType,
            ContactValue = contactValue.Trim(),
            ContactName = contactName?.Trim(),
            IsPublic = isPublic,
            DisplayOrder = displayOrder
        };
    }

    public void Update(ContactType contactType, string contactValue, string? contactName, bool isPublic, int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(contactValue) || displayOrder < 0)
            throw new CommerceDomainException("PRODUCER_CONTACT_INVALID", "Producer contact details are invalid.");
        ContactType = contactType;
        ContactValue = contactValue.Trim();
        ContactName = contactName?.Trim();
        IsPublic = isPublic;
        DisplayOrder = displayOrder;
    }

    private ProducerContact()
    {
    }
}
