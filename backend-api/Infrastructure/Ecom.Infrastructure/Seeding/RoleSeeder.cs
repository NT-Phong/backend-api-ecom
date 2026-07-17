using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using static Ecom.Domain.Constants.Permissions;

namespace Ecom.Infrastructure.Seeding;

public static class RoleSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("RoleSeeder");

        try
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // 1. Seed default roles
            var defaultRoles = new List<(string Code, string Name, string Description, int Priority, bool IsSystemRole)>
            {
                (Roles.Admin, "Admin", "Quản trị hệ thống", 1, true),
                (Roles.User, "Người dùng mới", "Người dùng tham khảo", 200, true)
            };

            var existingRoles = await db.Roles.ToListAsync(cancellationToken);
            foreach (var (code, name, description, priority, isSystemRole) in defaultRoles)
            {
                if (!existingRoles.Any(r => r.Code == code))
                {
                    db.Roles.Add(new Role
                    {
                        Code = code,
                        Name = name,
                        Description = description,
                        Priority = priority,
                        IsActive = true,
                        IsSystemRole = isSystemRole,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            await db.SaveChangesAsync(cancellationToken);
            var roles = await db.Roles.ToListAsync(cancellationToken);

            // 2. Fetch active policies and existing RolePolicies
            var allPolicies = await db.Policies.Where(p => p.IsActive).ToListAsync(cancellationToken);
            var allExistingRolePolicies = await db.RolePolicies.IgnoreQueryFilters().ToListAsync(cancellationToken);
            var rolePolicyLookup = allExistingRolePolicies
                .GroupBy(rp => rp.RoleId)
                .ToDictionary(g => g.Key, g => g.ToDictionary(rp => rp.PolicyId));

            bool hasChanges = false;

            foreach (var role in roles)
            {
                rolePolicyLookup.TryGetValue(role.Id, out var rolePoliciesMap);
                rolePoliciesMap ??= new Dictionary<Guid, RolePolicy>();

                foreach (var policy in allPolicies)
                {
                    string p = policy.Code.ToLower();

                    // Admin gets all policies; User only gets User.Read and User.Update
                    bool isGranted = false;
                    if (role.Code == Roles.Admin)
                    {
                        isGranted = true;
                    }
                    else if (role.Code == Roles.User)
                    {
                        isGranted = p == Permissions.User.Read.ToLower() || p == Permissions.User.Update.ToLower();
                    }

                    RolePolicy? existingRp = null;
                    rolePoliciesMap.TryGetValue(policy.Id, out existingRp);

                    if (isGranted)
                    {
                        if (existingRp == null)
                        {
                            db.RolePolicies.Add(new RolePolicy
                            {
                                RoleId = role.Id,
                                PolicyId = policy.Id,
                                CreatedAt = DateTime.UtcNow,
                                IsDeleted = false
                            });
                            hasChanges = true;
                        }
                        else if (existingRp.IsDeleted)
                        {
                            existingRp.IsDeleted = false;
                            existingRp.UpdatedAt = DateTime.UtcNow;
                            hasChanges = true;
                        }
                    }
                    else
                    {
                        if (existingRp != null && !existingRp.IsDeleted)
                        {
                            existingRp.IsDeleted = true;
                            existingRp.UpdatedAt = DateTime.UtcNow;
                            hasChanges = true;
                        }
                    }
                }
            }

            if (hasChanges)
            {
                await db.SaveChangesAsync(cancellationToken);
                logger.LogInformation("RoleSeeder: Roles and RolePolicies synchronization completed.");
            }
            else
            {
                logger.LogInformation("RoleSeeder: Permissions are synchronized, no changes needed.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RoleSeeder Error.");
        }
    }
}
