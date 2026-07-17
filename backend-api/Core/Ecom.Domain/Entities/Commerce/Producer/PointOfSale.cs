namespace Ecom.Domain.Entities;
public class PointOfSale : BaseEntity
{
    public Guid? ProducerId { get; private set; }
    public Guid? AdministrativeAreaId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string AddressLine { get; private set; } = string.Empty;
    public string? PhoneNumber { get; private set; }
    public decimal? Latitude { get; private set; }
    public decimal? Longitude { get; private set; }
    public string? OpeningHours { get; private set; }
    public PublicStatus PublicStatus { get; private set; }

    private PointOfSale()
    {
    }
}