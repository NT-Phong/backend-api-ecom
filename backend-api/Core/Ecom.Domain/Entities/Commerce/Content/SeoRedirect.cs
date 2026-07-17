namespace Ecom.Domain.Entities;
public class SeoRedirect : BaseEntity
{
    public string SourcePath { get; private set; } = string.Empty;
    public string TargetPath { get; private set; } = string.Empty;
    public int StatusCode { get; private set; } = 301;
    public bool IsActive { get; private set; } = true;

    private SeoRedirect()
    {
    }
}