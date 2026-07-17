using Ecom.Application.Common.Configuration;
using Ecom.Application.Common.Interfaces;
using Ecom.Domain.Interfaces;
using Ecom.Infrastructure.HealthChecks;
using Ecom.Infrastructure.Persistence.Database;
using Ecom.Infrastructure.Persistence.Database.Interceptors;
using Ecom.Infrastructure.Persistence.Database.UnitOfWork;
using Ecom.Infrastructure.Security.Authorization;
using Ecom.Infrastructure.Locking;
using Ecom.Infrastructure.Telemetry;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using StackExchange.Redis;
using System.Text;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ecom.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure settings with IOptions pattern
        services.Configure<ConnectionSettings>(configuration.GetSection(ConnectionSettings.SectionName));
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<OtpSettings>(configuration.GetSection(OtpSettings.SectionName));

        // Register OpenTelemetry
        services.AddOpenTelemetryServices(configuration);

        // Connection Service for Read/Write separation
        services.AddScoped<IConnectionService, ConnectionService>();

        // Database contexts
        var connectionSettings = new ConnectionSettings();
        configuration.GetSection(ConnectionSettings.SectionName).Bind(connectionSettings);
        var writeConnectionString = connectionSettings.DefaultConnection;

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(writeConnectionString);
        dataSourceBuilder.EnableDynamicJson();
        var dataSource = dataSourceBuilder.Build();

        services.AddScoped<AuditableEntityInterceptor>();
        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
            options.UseNpgsql(dataSource, serverOptions =>
                {
                    serverOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    serverOptions.CommandTimeout(30); // 30 second timeout
                })
                .EnableSensitiveDataLogging(false)
                .AddInterceptors(serviceProvider.GetRequiredService<AuditableEntityInterceptor>()));

        // Repositories
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Auto-register repository implementations
        services.RegisAllService(new[] { "Ecom.Infrastructure", "Ecom.Application" });

        // Services
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<IDateTimeService, DateTimeService>();

        // Authentication & Security Services
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        #region JwtSettings

        var jwtSettings = new JwtSettings();
        configuration.GetSection(JwtSettings.SectionName).Bind(jwtSettings);

        if (!string.IsNullOrEmpty(jwtSettings.SecretKey) && jwtSettings.SecretKey.Length >= 32)
        {
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = jwtSettings.ValidateIssuer,
                        ValidateAudience = jwtSettings.ValidateAudience,
                        ValidateLifetime = jwtSettings.ValidateLifetime,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                        ClockSkew = TimeSpan.FromSeconds(jwtSettings.ClockSkewSeconds)
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            if (context.Exception is SecurityTokenExpiredException)
                            {
                                context.Response.Headers["Token-Expired"] = "true";
                            }

                            return Task.CompletedTask;
                        },

                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                            {
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddAuthorization();

            // Register custom authorization policy provider and handler for dynamic permission-based policies
            services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();
            services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        }

        #endregion

        // Redis setup:
        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redisConnection))
        {
            var redisOptions = ConfigurationOptions.Parse(redisConnection);
            redisOptions.AbortOnConnectFail = false;
            redisOptions.ConnectTimeout = 10000;
            redisOptions.AsyncTimeout = 10000;
            redisOptions.SyncTimeout = 10000;
            redisOptions.ConnectRetry = 5;

            redisOptions.ReconnectRetryPolicy = new ExponentialRetry(5000);

            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                try
                {
                    return ConnectionMultiplexer.Connect(redisOptions);
                }
                catch (Exception ex)
                {
                    var logger = sp.GetRequiredService<ILogger<IConnectionMultiplexer>>();
                    logger.LogError(ex, "Could not connect to Redis at startup.");
                    return ConnectionMultiplexer.Connect(redisOptions);
                }
            });

            services.AddStackExchangeRedisCache(options =>
            {
                options.ConfigurationOptions = redisOptions;
                options.InstanceName = "Starter:";
            });

            services.AddSingleton<IDistributedLockService, InMemoryDistributedLockService>();
        }
        else
        {
            services.AddDistributedMemoryCache();
            services.AddSingleton<IDistributedLockService, InMemoryDistributedLockService>();
        }

        services.AddScoped<IDistributedCacheService, DistributedCacheService>();

        // Health Checks
        services.AddScoped<LivenessHealthCheck>();
        services.AddScoped<ReadinessHealthCheck>();
    }

    public static IServiceCollection RegisAllService(this IServiceCollection services, string[]? projects,
        string[]? ignoreProjects = null)
    {
        if (projects == null || projects.Length == 0)
        {
            return services;
        }

        var projectSet = new HashSet<string>(projects.Where(p => !string.IsNullOrWhiteSpace(p)));
        var ignoreSet = ignoreProjects != null ? new HashSet<string>(ignoreProjects) : new HashSet<string>();

        var assemblies = new List<Assembly>();
        var loaded = AppDomain.CurrentDomain.GetAssemblies().ToList();
        foreach (var proj in projectSet)
        {
            Assembly? asm = loaded.FirstOrDefault(a =>
                string.Equals(a.GetName().Name, proj, StringComparison.OrdinalIgnoreCase));
            if (asm != null)
            {
                assemblies.Add(asm);
                continue;
            }

            try
            {
                var loadedAsm = Assembly.Load(new AssemblyName(proj));
                assemblies.Add(loadedAsm);
            }
            catch
            {
                // Ignore assembly load issues
            }
        }

        foreach (var assembly in assemblies.Distinct())
        {
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException rtle)
            {
                types = rtle.Types.Where(t => t != null).ToArray()!;
            }
            catch
            {
                continue;
            }

            var repoCandidates = types.Where(t =>
                    t.IsClass
                    && !t.IsAbstract
                    && t.Name.EndsWith("Repository", StringComparison.OrdinalIgnoreCase)
                )
                .ToList();

            if (!repoCandidates.Any())
                continue;

            foreach (var type in repoCandidates)
            {
                if (ignoreSet.Contains(type.Name))
                {
                    continue;
                }

                try
                {
                    var baseInterfaces = type.BaseType?.GetInterfaces() ?? Array.Empty<Type>();

                    var candidateInterfaces = type.GetInterfaces()
                        .Where(i => i.Name.EndsWith("Repository", StringComparison.OrdinalIgnoreCase) &&
                                    !baseInterfaces.Contains(i))
                        .ToList();

                    if (!candidateInterfaces.Any())
                        candidateInterfaces = type.GetInterfaces().Where(i => !baseInterfaces.Contains(i)).ToList();

                    if (!candidateInterfaces.Any())
                    {
                        continue;
                    }

                    foreach (var iface in candidateInterfaces)
                    {
                        if (iface.IsGenericTypeDefinition) continue;

                        services.AddScoped(iface, type);
                    }
                }
                catch
                {
                    // Ignore registration issues
                }
            }
        }

        return services;
    }
}

