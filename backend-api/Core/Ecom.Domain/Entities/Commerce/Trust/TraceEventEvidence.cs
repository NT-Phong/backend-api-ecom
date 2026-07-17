namespace Ecom.Domain.Entities;
public class TraceEventEvidence : BaseEntity
{
    public Guid TraceEventId { get; private set; }
    public Guid MediaAssetId { get; private set; }

    private TraceEventEvidence()
    {
    }
}