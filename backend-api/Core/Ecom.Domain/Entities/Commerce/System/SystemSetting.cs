namespace Ecom.Domain.Entities;
public class SystemSetting : BaseEntity
{
    public string SettingKey { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;
    public bool IsPublic { get; private set; }
    public string? Description { get; private set; }

    public static SystemSetting Create(string settingKey, string value, bool isPublic = false, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(settingKey) || string.IsNullOrWhiteSpace(value))
            throw new CommerceDomainException("SYSTEM_SETTING_REQUIRED", "Setting key and value are required.");
        return new SystemSetting { SettingKey = settingKey.Trim(), Value = value.Trim(), IsPublic = isPublic, Description = description?.Trim() };
    }

    public void UpdateValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new CommerceDomainException("SYSTEM_SETTING_VALUE_REQUIRED", "Setting value is required.");
        Value = value.Trim();
    }

    private SystemSetting()
    {
    }
}
