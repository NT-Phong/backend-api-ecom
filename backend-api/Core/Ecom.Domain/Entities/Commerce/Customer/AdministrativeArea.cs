namespace Ecom.Domain.Entities;
public class AdministrativeArea : BaseEntity
{
    public Guid? ParentId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public AdministrativeAreaLevel Level { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; } = true;

    private AdministrativeArea()
    {
    }
}