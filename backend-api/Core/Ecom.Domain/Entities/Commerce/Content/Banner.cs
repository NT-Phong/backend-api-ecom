namespace Ecom.Domain.Entities;
public class Banner : BaseEntity
{
    public Guid? CampaignId { get; private set; }
    public Guid MediaAssetId { get; private set; }
    public string? Title { get; private set; }
    public string AltText { get; private set; } = string.Empty;
    public string? TargetUrl { get; private set; }
    public DateTime? StartsAt { get; private set; }
    public DateTime? EndsAt { get; private set; }
    public int DisplayOrder { get; private set; }
    public ContentStatus Status { get; private set; }

    private Banner()
    {
    }
}