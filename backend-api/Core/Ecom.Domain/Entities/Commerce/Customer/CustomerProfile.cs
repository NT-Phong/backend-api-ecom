namespace Ecom.Domain.Entities;
public class CustomerProfile : BaseEntity
{
    public Guid UserId { get; private set; }
    public string? PreferredName { get; private set; }
    public DateOnly? DateOfBirth { get; private set; }
    public string? Gender { get; private set; }
    public DateTime? MarketingConsentAt { get; private set; }

    private CustomerProfile()
    {
    }
}