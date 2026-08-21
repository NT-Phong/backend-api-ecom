using Ecom.Application.Common.Configuration;
using Ecom.Application.Common.Interfaces;
using Ecom.Domain.Interfaces;
using Ecom.Domain.Enums;
using Ecom.Infrastructure.HealthChecks;
using Ecom.Infrastructure.Persistence.Database;
using Ecom.Infrastructure.Persistence.Database.Interceptors;
using Ecom.Infrastructure.Persistence.Database.UnitOfWork;
using Ecom.Infrastructure.Security.Authorization;
using Ecom.Infrastructure.Locking;
using Ecom.Infrastructure.Services;
using Ecom.Infrastructure.Telemetry;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using StackExchange.Redis;
using System.Text;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Ecom.Infrastructure.Services.Sms;

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
        services.Configure<AuthRateLimitOptions>(configuration.GetSection(AuthRateLimitOptions.SectionName));
        services.Configure<MediaStorageOptions>(configuration.GetSection(MediaStorageOptions.SectionName));
        services.Configure<MediaProcessingOptions>(configuration.GetSection(MediaProcessingOptions.SectionName));
        services.Configure<OutboxProcessorOptions>(configuration.GetSection(OutboxProcessorOptions.SectionName));
        services.AddSingleton<IValidateOptions<DemoQrLoginOptions>, DemoQrLoginOptionsValidator>();
        services.AddOptions<DemoQrLoginOptions>()
            .Bind(configuration.GetSection(DemoQrLoginOptions.SectionName))
            .ValidateOnStart();
        services.Configure<PasswordSettings>(configuration.GetSection(PasswordSettings.SectionName));
        services.Configure<EmailVerificationOptions>(configuration.GetSection(EmailVerificationOptions.SectionName));
        services.AddSingleton<IValidateOptions<SePayOptions>, SePayOptionsValidator>();
        services.AddOptions<SePayOptions>()
            .Bind(configuration.GetSection(SePayOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<SePayBankQrOptions>, SePayBankQrOptionsValidator>();
        services.AddOptions<SePayBankQrOptions>()
            .Bind(configuration.GetSection(SePayBankQrOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<PasswordAuthenticationV2Options>, PasswordAuthenticationV2OptionsValidator>();
        services.AddOptions<PasswordAuthenticationV2Options>()
            .Bind(configuration.GetSection(PasswordAuthenticationV2Options.SectionName))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<OtpSettings>, OtpSettingsValidator>();
        services.AddSingleton<IValidateOptions<PasswordSettings>, PasswordSettingsValidator>();
        services.AddOptions<OtpSettings>()
            .Bind(configuration.GetSection(OtpSettings.SectionName))
            .ValidateOnStart();
        services.AddOptions<PasswordSettings>()
            .Bind(configuration.GetSection(PasswordSettings.SectionName))
            .ValidateOnStart();

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
        services.AddScoped<ConvertDomainEventsToOutboxInterceptor>();
        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            options.UseNpgsql(dataSource, serverOptions =>
                {
                    serverOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    serverOptions.CommandTimeout(30); // 30 second timeout
                })
                .EnableSensitiveDataLogging(false)
                .AddInterceptors(serviceProvider.GetRequiredService<AuditableEntityInterceptor>());

            // Keep disabled until the outbox migration and PostgreSQL atomicity tests pass.
            if (configuration.GetValue<bool>("Outbox:Enabled"))
                options.AddInterceptors(serviceProvider.GetRequiredService<ConvertDomainEventsToOutboxInterceptor>());
        });

        // Repositories
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ICartPrincipalResolver, CartPrincipalResolver>();
        services.AddScoped<ICheckoutCartStore, CheckoutCartStore>();
        services.AddScoped<ICartMutationLock, CartMutationLock>();
        services.AddScoped<IInventoryReservationStore, InventoryReservationStore>();
        services.AddScoped<IOrderLifecycleStore, OrderLifecycleStore>();
        services.AddScoped<IIdempotencyStore, IdempotencyStore>();
        services.AddScoped<IPublicCatalogReadStore, PublicCatalogReadStore>();
        services.AddScoped<IManagementDashboardReadStore, ManagementDashboardReadStore>();
        services.AddSingleton<IOrderNumberGenerator, OrderNumberGenerator>();
        services.AddScoped<OutboxMessageDispatcher>();
        services.AddScoped<OutboxProcessor>();

        // Auto-register repository implementations
        services.RegisAllService(new[] { "Ecom.Infrastructure", "Ecom.Application" });

        // Services
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<IDateTimeService, DateTimeService>();
        services.AddScoped<IHelperService, HelperService>();
        services.AddSingleton<ISePayCheckoutService, SePayCheckoutService>();
        services.AddSingleton<ISePayBankQrService, SePayBankQrService>();
        if (string.Equals(configuration[$"{MediaStorageOptions.SectionName}:Provider"], "Azure", StringComparison.OrdinalIgnoreCase))
            services.AddScoped<IStorageService, AzureBlobStorageService>();
        else
            services.AddScoped<IStorageService, LocalStorageService>();
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IFileUploadPolicy, FileUploadPolicy>();
        services.AddScoped<IMediaFileService, MediaFileService>();
        services.AddSingleton<IMalwareScanner, ClamAvMalwareScanner>();
        services.AddHostedService<MediaStorageStartupValidator>();
        services.AddHostedService<MediaProcessingWorker>();
        services.AddHostedService<ReservationExpiryWorker>();
        if (configuration.GetValue<bool>("Outbox:Enabled"))
            services.AddHostedService<OutboxProcessorWorker>();

        // Authentication & Security Services
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<IOtpSecurityService, OtpSecurityService>();
        services.AddSingleton<IAuthTokenProtector, AuthTokenProtector>();
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<ISessionRefreshService, SessionRefreshService>();
        services.AddSingleton<ISmsSender, SmsSender>();

        #region JwtSettings

        var jwtSettings = new JwtSettings();
        configuration.GetSection(JwtSettings.SectionName).Bind(jwtSettings);

        if (!string.IsNullOrEmpty(jwtSettings.SecretKey) && jwtSettings.SecretKey.Length >= 32)
        {
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "ecom-auth";
                    options.DefaultChallengeScheme = "ecom-auth";
                    options.DefaultScheme = "ecom-auth";
                })
                .AddPolicyScheme("ecom-auth", "JWT or session cookie", options => options.ForwardDefaultSelector = context =>
                    context.Request.Headers.Authorization.ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                        ? JwtBearerDefaults.AuthenticationScheme : CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.Cookie.Name = "__Host-ecom_session"; options.Cookie.HttpOnly = true; options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.Cookie.SameSite = SameSiteMode.Lax; options.Cookie.Path = "/"; options.SlidingExpiration = false;
                    options.Events.OnValidatePrincipal = async context =>
                    {
                        var sid = context.Principal?.FindFirst("session_id")?.Value;
                        var uid = context.Principal?.FindFirst("userId")?.Value;
                        if (!Guid.TryParse(sid,out var sessionId) || !Guid.TryParse(uid,out var userId)) { context.RejectPrincipal(); return; }
                        var db=context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>(); var now=DateTime.UtcNow;
                        var user=await db.Users.AsNoTracking().FirstOrDefaultAsync(x=>x.Id==userId,context.HttpContext.RequestAborted);
                        var session=await db.UserSessions.AsNoTracking().FirstOrDefaultAsync(x=>x.Id==sessionId && x.UserId==userId,context.HttpContext.RequestAborted);
                        if(user is null || session is null || user.Status!=UserStatusEnum.Active || !session.IsActive(now,user.SecurityStamp)) context.RejectPrincipal();
                    };
                    options.Events.OnRedirectToLogin = c => { c.Response.StatusCode=StatusCodes.Status401Unauthorized; return Task.CompletedTask; };
                    options.Events.OnRedirectToAccessDenied = c => { c.Response.StatusCode=StatusCodes.Status403Forbidden; return Task.CompletedTask; };
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

                        OnChallenge = async context =>
                        {
                            if (context.Response.HasStarted) return;
                            context.HandleResponse();
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            context.Response.ContentType = "application/json";
                            await context.Response.WriteAsJsonAsync(
                                ApiResponse<object>.Fail(MessageKey.Unauthorized, ErrorCodes.UNAUTHORIZED),
                                context.HttpContext.RequestAborted);
                        },

                        OnForbidden = async context =>
                        {
                            if (context.Response.HasStarted) return;
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            context.Response.ContentType = "application/json";
                            await context.Response.WriteAsJsonAsync(
                                ApiResponse<object>.Fail(MessageKey.Forbidden, ErrorCodes.FORBIDDEN),
                                context.HttpContext.RequestAborted);
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

        // Distributed auth rate limits: Development can explicitly choose process-local counters;
        // production must either use Redis or fail closed.
        var authRateLimitSettings = configuration.GetSection(AuthRateLimitOptions.SectionName)
            .Get<AuthRateLimitOptions>() ?? new AuthRateLimitOptions();
        var redisConnection = configuration.GetConnectionString("Redis");
        if (authRateLimitSettings.Backend == AuthRateLimitBackend.InMemory)
        {
            services.AddDistributedMemoryCache();
            services.AddSingleton<IDistributedLockService, InMemoryDistributedLockService>();
            services.AddSingleton<IAuthRateLimitCounterStore, InMemoryAuthRateLimitCounterStore>();
            services.AddSingleton<IAuthRateLimitService, DistributedAuthRateLimitService>();
        }
        else if (authRateLimitSettings.Backend == AuthRateLimitBackend.Redis && !string.IsNullOrWhiteSpace(redisConnection))
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

            services.AddSingleton<IDistributedLockService, RedisDistributedLockService>();
            services.AddSingleton<IAuthRateLimitCounterStore, RedisAuthRateLimitCounterStore>();
            services.AddSingleton<IAuthRateLimitService, DistributedAuthRateLimitService>();
        }
        else
        {
            services.AddDistributedMemoryCache();
            services.AddSingleton<IDistributedLockService, InMemoryDistributedLockService>();
            services.AddSingleton<IAuthRateLimitService, UnavailableAuthRateLimitService>();
        }

        services.AddScoped<IDistributedCacheService, DistributedCacheService>();
        services.AddSingleton<IDemoQrLoginStore, DemoQrLoginStore>();

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

