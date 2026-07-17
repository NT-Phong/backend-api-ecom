using Microsoft.AspNetCore.Authorization;

namespace Ecom.Infrastructure.Security.Authorization;

/// <summary>
/// Requirement cho việc kiểm tra permission/policy
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }
}

