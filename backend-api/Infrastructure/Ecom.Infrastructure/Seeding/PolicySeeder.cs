using Ecom.Application.Common.Configuration;
using Ecom.Domain.Constants;
using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ecom.Infrastructure.Seeding;

public static class PolicySeeder
{
    /// <summary>
    /// Seed policies defined in code (Permissions.GetAll()) into Policy table
    /// - Insert missing policies
    /// - (Optional) Update Name/Module if changed
    /// </summary>
    public static async Task SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("PolicySeeder");
        try
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var allPermissions = Permissions.GetAll();
            var definedCodes = allPermissions.Select(p => p.Code).ToList();

            // Xóa (Soft Delete) hoặc vô hiệu hóa các Policy cũ trong DB KHÔNG CÒN nằm trong định nghĩa mới
            var existingPoliciesInDb = await db.Policies.ToListAsync(cancellationToken);
            foreach (var existingPolicy in existingPoliciesInDb)
            {
                if (!definedCodes.Contains(existingPolicy.Code) && existingPolicy.IsActive)
                {
                    existingPolicy.IsActive = false; // Hoặc existingPolicy.IsDeleted = true; nếu bạn dùng Soft Delete
                    existingPolicy.UpdatedAt = DateTime.UtcNow;
                }
                else if (definedCodes.Contains(existingPolicy.Code) && !existingPolicy.IsActive)
                {
                    // Phục hồi nguyên trạng nếu Policy đó quay trở lại mã nguồn
                    existingPolicy.IsActive = true;
                    existingPolicy.UpdatedAt = DateTime.UtcNow;
                }
            }

            foreach (var p in allPermissions)
            {
                var exists = await db.Policies
                    .AsQueryable()
                    .FirstOrDefaultAsync(x => x.Code == p.Code, cancellationToken);

                if (exists == null)
                {
                    db.Policies.Add(new Policy
                    {
                        Code = p.Code,
                        Name = p.Name,
                        Module = p.Module,
                        IsActive = true,
                        IsSystemPolicy = true,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                else
                {
                    // Optionally update metadata if changed
                    var changed = false;
                    if (exists.Name != p.Name)
                    {
                        exists.Name = p.Name;
                        changed = true;
                    }
                    if (exists.Module != p.Module)
                    {
                        exists.Module = p.Module;
                        changed = true;
                    }
                    if (changed)
                    {
                        exists.UpdatedAt = DateTime.UtcNow;
                    }
                }
            }

            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Policy seeding completed. Total policies: {Count}", (await db.Policies.CountAsync(cancellationToken)));
        }
        catch (Exception ex)
        {
            var logger2 = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("PolicySeeder");
            logger2.LogError(ex, "Error while seeding policies");
        }
    }
}

