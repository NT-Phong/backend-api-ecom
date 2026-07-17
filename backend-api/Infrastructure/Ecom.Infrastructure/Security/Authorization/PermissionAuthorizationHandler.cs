using Microsoft.AspNetCore.Authorization;

namespace Ecom.Infrastructure.Security.Authorization;

/// <summary>
/// Handler để kiểm tra user có permission/policy trong JWT claims không
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // Kiểm tra user có claim "policy" với giá trị tương ứng không
        var policyClaims = context.User.FindAll("policy");
        
        if (policyClaims.Any(c => c.Value == requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

