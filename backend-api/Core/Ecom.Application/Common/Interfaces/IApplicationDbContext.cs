using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Ecom.Domain.Entities;

namespace Ecom.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    // Auth entities
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<Policy> Policies { get; }
    DbSet<RolePolicy> RolePolicies { get; }
    DbSet<UserPolicy> UserPolicies { get; }
    DbSet<JwtRefreshToken> JwtRefreshTokens { get; }
    DbSet<OtpToken> OtpTokens { get; }
    DbSet<UserDeviceToken> UserDeviceTokens { get; }

    // Database operations
    DatabaseFacade Database { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

