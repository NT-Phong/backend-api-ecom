namespace Ecom.Domain.Entities;
public class PartnerApplication : BaseEntity
{
    public Guid? UserId { get; private set; }
    public string ApplicantName { get; private set; } = string.Empty;
    public string? OrganizationName { get; private set; }
    public string? Email { get; private set; }
    public string PhoneNumber { get; private set; } = string.Empty;
    public PartnerApplicationType ApplicationType { get; private set; }
    public PartnerApplicationStatus Status { get; private set; }
    public string? Message { get; private set; }
    public Guid? AssignedToUserId { get; private set; }

    private PartnerApplication()
    {
    }
}