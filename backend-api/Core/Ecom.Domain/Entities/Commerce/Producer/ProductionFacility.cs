namespace Ecom.Domain.Entities;
public class ProductionFacility : BaseEntity
{
    public Guid ProducerId { get; private set; }
    public Guid? AdministrativeAreaId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? AddressLine { get; private set; }
    public decimal? Latitude { get; private set; }
    public decimal? Longitude { get; private set; }
    public PublicStatus PublicStatus { get; private set; }
    public string? Description { get; private set; }

    public static ProductionFacility Create(Guid producerId, Guid? administrativeAreaId, string name, string? addressLine,
        decimal? latitude, decimal? longitude, string? description)
    {
        var facility = new ProductionFacility { ProducerId = producerId, PublicStatus = PublicStatus.Draft };
        facility.UpdateDetails(administrativeAreaId, name, addressLine, latitude, longitude, description);
        return facility;
    }

    public void UpdateDetails(Guid? administrativeAreaId, string name, string? addressLine, decimal? latitude,
        decimal? longitude, string? description)
    {
        if (PublicStatus == PublicStatus.Hidden)
            throw new CommerceDomainException("FACILITY_HIDDEN", "A hidden facility cannot be updated.");
        if (string.IsNullOrWhiteSpace(name) || latitude is < -90 or > 90 || longitude is < -180 or > 180)
            throw new CommerceDomainException("FACILITY_DETAILS_INVALID", "Production facility details are invalid.");
        AdministrativeAreaId = administrativeAreaId;
        Name = name.Trim();
        AddressLine = addressLine?.Trim();
        Latitude = latitude;
        Longitude = longitude;
        Description = description?.Trim();
    }

    public void Hide() => PublicStatus = PublicStatus.Hidden;

    private ProductionFacility()
    {
    }
}
