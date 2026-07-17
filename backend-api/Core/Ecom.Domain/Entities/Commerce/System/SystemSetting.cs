namespace Ecom.Domain.Entities;
public class SystemSetting : BaseEntity
{
    public string SettingKey { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;
    public bool IsPublic { get; private set; }
    public string? Description { get; private set; }

    private SystemSetting()
    {
    }
}