namespace Ecom.Domain.Entities;

/// <summary>
/// Join table cho quan hệ many-to-many giữa Role và Policy
/// 1 Role có nhiều Policy, 1 Policy có thể thuộc nhiều Role
/// </summary>
public class RolePolicy : BaseEntity
{
    /// <summary>
    /// Role ID
    /// </summary>
    public Guid RoleId { get; set; }
    
    /// <summary>
    /// Policy ID
    /// </summary>
    public Guid PolicyId { get; set; }
    
    #region Navigation Properties
    
    /// <summary>
    /// Navigation đến Role
    /// </summary>
    public virtual Role? Role { get; set; }
    
    /// <summary>
    /// Navigation đến Policy
    /// </summary>
    public virtual Policy? Policy { get; set; }
    
    #endregion
}

