namespace Ecom.Domain.Entities;
public class PageSectionProduct : BaseEntity
{
    public Guid PageSectionId { get; private set; }
    public Guid ProductId { get; private set; }
    public int DisplayOrder { get; private set; }

    private PageSectionProduct()
    {
    }
}