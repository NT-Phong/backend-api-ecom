namespace Ecom.Application.Common.Interfaces;

/// <summary>
/// Interface cho Password Hashing Service
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hash password với BCrypt
    /// </summary>
    /// <param name="password">Plain text password</param>
    /// <returns>Hashed password</returns>
    string HashPassword(string password);
    
    /// <summary>
    /// Verify password với hash đã lưu
    /// </summary>
    /// <param name="password">Plain text password</param>
    /// <param name="hashedPassword">Hash đã lưu trong database</param>
    /// <returns>True nếu password đúng</returns>
    bool VerifyPassword(string password, string hashedPassword);
    
    /// <summary>
    /// Kiểm tra hash có cần được rehash không (do work factor thay đổi)
    /// </summary>
    /// <param name="hashedPassword">Hash hiện tại</param>
    /// <returns>True nếu cần rehash</returns>
    bool NeedsRehash(string hashedPassword);
}

