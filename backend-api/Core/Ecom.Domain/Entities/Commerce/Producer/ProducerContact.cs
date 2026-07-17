namespace Ecom.Domain.Entities;
public class ProducerContact : BaseEntity
{
    public Guid ProducerId { get; private set; }
    public ContactType ContactType { get; private set; }
    public string ContactValue { get; private set; } = string.Empty;
    public string? ContactName { get; private set; }
    public bool IsPublic { get; private set; }
    public int DisplayOrder { get; private set; }

    private ProducerContact()
    {
    }
}