namespace Ecom.Domain.Entities;
public class StockLocation : BaseEntity
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public Guid? AdministrativeAreaId { get; private set; }
    public string? AddressLine { get; private set; }
    public bool IsActive { get; private set; } = true;

    public static StockLocation Create(string code, string name, Guid? administrativeAreaId, string? addressLine)
    {
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
            throw new CommerceDomainException("STOCK_LOCATION_REQUIRED", "Location code and name are required.");
        return new StockLocation { Code = code.Trim(), Name = name.Trim(), AdministrativeAreaId = administrativeAreaId, AddressLine = addressLine?.Trim(), IsActive = true };
    }

    public void UpdateDetails(string name, Guid? administrativeAreaId, string? addressLine)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new CommerceDomainException("STOCK_LOCATION_NAME_REQUIRED", "Location name is required.");
        Name = name.Trim();
        AdministrativeAreaId = administrativeAreaId;
        AddressLine = addressLine?.Trim();
    }

    public void SetActive(bool isActive) => IsActive = isActive;

    private StockLocation()
    {
    }
}
