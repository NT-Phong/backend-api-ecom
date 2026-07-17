namespace Ecom.Domain.Entities;
public class TraceProfile : BaseEntity
{
    public Guid ProductId { get; private set; }
    public string PublicCode { get; private set; } = string.Empty;
    public string? Summary { get; private set; }
    public PublicStatus PublicStatus { get; private set; }

    private TraceProfile()
    {
    }
}