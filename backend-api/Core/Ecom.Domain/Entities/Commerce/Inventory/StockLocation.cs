namespace Ecom.Domain.Entities;
public class StockLocation : BaseEntity
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public Guid? AdministrativeAreaId { get; private set; }
    public string? AddressLine { get; private set; }
    public bool IsActive { get; private set; } = true;

    private StockLocation()
    {
    }
}