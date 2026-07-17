namespace Ecom.Domain.Entities;
public class TraceLot : BaseEntity
{
    public Guid TraceProfileId { get; private set; }
    public string LotCode { get; private set; } = string.Empty;
    public DateOnly? ProducedAt { get; private set; }
    public DateOnly? ExpiresAt { get; private set; }
    public TraceLotStatus Status { get; private set; }

    private TraceLot()
    {
    }
}