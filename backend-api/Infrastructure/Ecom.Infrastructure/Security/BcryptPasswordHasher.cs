using BCrypt.Net;
using Ecom.Application.Common.Configuration;
using Ecom.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace Ecom.Infrastructure.Security;

/// <summary>
/// Password Hashing Service sử dụng BCrypt
/// BCrypt tự động bao gồm salt trong hash output
/// </summary>
public class BcryptPasswordHasher : IPasswordHasher
{
    private readonly PasswordSettings _settings;
    
    public BcryptPasswordHasher(IOptions<PasswordSettings> settings)
    {
        _settings = settings.Value;
    }
    
    /// <inheritdoc />
    public string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            throw new ArgumentException("Password cannot be null or empty", nameof(password));
        }
        
        // BCrypt.HashPassword tự động tạo salt và hash
        // Work factor được lấy từ config
        return BCrypt.Net.BCrypt.HashPassword(password, _settings.BcryptWorkFactor);
    }
    
    /// <inheritdoc />
    public bool VerifyPassword(string password, string hashedPassword)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword))
        {
            return false;
        }
        
        try
        {
            // BCrypt.Verify so sánh password với hash đã lưu
            // Salt được extract từ hash string
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
        catch (SaltParseException)
        {
            // Hash không đúng format BCrypt
            return false;
        }
        catch (Exception)
        {
            // Các lỗi khác (hash corrupted, etc.)
            return false;
        }
    }
    
    /// <inheritdoc />
    public bool NeedsRehash(string hashedPassword)
    {
        if (string.IsNullOrEmpty(hashedPassword))
        {
            return true;
        }
        
        try
        {
            // BCrypt hash format: $2a$XX$... hoặc $2b$XX$...
            // XX là work factor (2 digits)
            // Ví dụ: $2a$12$... có work factor = 12
            
            if (hashedPassword.Length < 7)
            {
                return true;
            }
            
            // Extract work factor từ hash
            var parts = hashedPassword.Split('$');
            if (parts.Length < 3)
            {
                return true;
            }
            
            if (int.TryParse(parts[2], out int currentWorkFactor))
            {
                // Cần rehash nếu work factor hiện tại nhỏ hơn config
                return currentWorkFactor < _settings.BcryptWorkFactor;
            }
            
            return true;
        }
        catch
        {
            return true;
        }
    }
}

