namespace Ecom.Application.Common.Configuration;

/// <summary>
/// Cấu hình Password Hashing
/// </summary>
public class PasswordSettings
{
    public const string SectionName = "Password";
    
    /// <summary>
    /// Work factor cho BCrypt (số lần iteration = 2^WorkFactor)
    /// Range: 4-31, Default: 12 (tương đương ~4096 iterations)
    /// Giá trị cao hơn = an toàn hơn nhưng chậm hơn
    /// </summary>
    public int BcryptWorkFactor { get; set; } = 12;
    
    /// <summary>
    /// Độ dài tối thiểu của mật khẩu
    /// </summary>
    public int MinLength { get; set; } = 15;
    
    /// <summary>
    /// Độ dài tối đa của mật khẩu (để tránh DoS attack với password dài)
    /// </summary>
    public int MaxLength { get; set; } = 128;
    
    /// <summary>
    /// Yêu cầu có chữ hoa
    /// </summary>
    public bool RequireUppercase { get; set; } = false;
    
    /// <summary>
    /// Yêu cầu có chữ thường
    /// </summary>
    public bool RequireLowercase { get; set; } = false;
    
    /// <summary>
    /// Yêu cầu có số
    /// </summary>
    public bool RequireDigit { get; set; } = false;
    
    /// <summary>
    /// Yêu cầu có ký tự đặc biệt
    /// </summary>
    public bool RequireSpecialCharacter { get; set; } = false;
    
    /// <summary>
    /// Danh sách ký tự đặc biệt được chấp nhận
    /// </summary>
    public string AllowedSpecialCharacters { get; set; } = "!@#$%^&*()_+-=[]{}|;':\",./<>?";
}

