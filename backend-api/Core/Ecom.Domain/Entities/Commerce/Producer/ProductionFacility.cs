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

    private ProductionFacility()
    {
    }
}