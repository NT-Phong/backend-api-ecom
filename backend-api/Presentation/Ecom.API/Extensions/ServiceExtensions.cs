using Ecom.API.Filters;
using System.Text.Json.Serialization;
using Ecom.API.Serialization;
using Microsoft.OpenApi.Models;
using Ecom.Application.Common.Configuration;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace Ecom.API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers(options =>
        {
            options.Filters.Add<ModelValidationFilter>();
        })
        .AddJsonOptions(o =>
        {
            o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            // Ensure DateTime values are serialized/deserialized as UTC ISO-8601 (with Z)
            o.JsonSerializerOptions.Converters.Add(new DateTimeUtcConverter());
            o.JsonSerializerOptions.Converters.Add(new NullableDateTimeUtcConverter());
        });

        // Disable automatic model validation to use our custom filter
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });

        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.ConfigureOptions<ConfigureSwaggerOptions>();

        services.AddCors(options =>
        {
            options.AddPolicy("DefaultPolicy", builder =>
            {
                builder.WithOrigins("http://localhost:3000", "https://localhost:3000", "http://localhost:5173", "https://test-portal.Ecom.vn", "https://mebi-mebione-d-portal-as-1.azurewebsites.net", "https://Ecom-log-viewer.vercel.app")
                       .AllowAnyMethod()
                       .AllowAnyHeader()
                       .AllowCredentials();
            });
        });
        services.Configure<AuthResponseTimingOptions>(
            configuration.GetSection(AuthResponseTimingOptions.SectionName));
        services.AddAntiforgery(o => { o.HeaderName = "X-CSRF-TOKEN"; o.Cookie.Name = "__Host-ecom_csrf"; o.Cookie.SecurePolicy = CookieSecurePolicy.Always; o.Cookie.SameSite = SameSiteMode.Lax; });

        AddAuthenticationRateLimiting(services, configuration);

        // Health checks
        var healthChecksBuilder = services.AddHealthChecks()
            .AddCheck("service",
                () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Application is running"),
                tags: new[] { "live" });

        // Guard the DB health check registration with a clear message if the connection string is missing
        var defaultConnection = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrWhiteSpace(defaultConnection))
        {
            // Register DB health check only when connection string is provided
            healthChecksBuilder.AddNpgSql(defaultConnection, tags: new[] { "ready" });
        }
        else
        {
            // Fail fast with an actionable exception so the operator can fix configuration
            throw new InvalidOperationException(
                "Missing connection string 'DefaultConnection'. Please configure it in appsettings.json or via environment variable 'ConnectionStrings__DefaultConnection'.");
        }

        return services;
    }

    private static void AddAuthenticationRateLimiting(IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection(AuthRateLimitOptions.SectionName)
            .Get<AuthRateLimitOptions>() ?? new AuthRateLimitOptions();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.ContentType = "application/json";
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    context.HttpContext.Response.Headers.RetryAfter = Math.Max(1, (int)Math.Ceiling(retryAfter.TotalSeconds)).ToString();

                await context.HttpContext.Response.WriteAsJsonAsync(
                    ApiResponse<object>.Fail(MessageKey.TooManyRequests, ErrorCodes.TOO_MANY_REQUESTS),
                    cancellationToken);
            };

            AddIpPolicy(options, AuthRateLimitPolicyNames.RegisterIp, settings.RegisterIp);
            AddIpPolicy(options, AuthRateLimitPolicyNames.OtpSendIp, settings.OtpSendIp);
            AddIpPolicy(options, AuthRateLimitPolicyNames.OtpVerifyIp, settings.OtpVerifyIp);
            AddIpPolicy(options, AuthRateLimitPolicyNames.RefreshIp, settings.RefreshIp);
            AddIpPolicy(options, AuthRateLimitPolicyNames.PasswordLoginIp, settings.PasswordLoginIp);
        });
    }

    private static void AddIpPolicy(RateLimiterOptions options, string name, RateLimitRule rule)
    {
        options.AddPolicy(name, httpContext => RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = Math.Max(1, rule.PermitLimit),
                Window = TimeSpan.FromSeconds(Math.Max(1, rule.WindowSeconds)),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
    }
    // Helper for Swagger
    public class SwaggerDefaultValues : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var apiDescription = context.ApiDescription;
            operation.Deprecated |= apiDescription.IsDeprecated();

            if (operation.Parameters == null) return;

            foreach (var parameter in operation.Parameters)
            {
                var description = apiDescription.ParameterDescriptions.First(p => p.Name == parameter.Name);
                if (parameter.Description == null)
                {
                    parameter.Description = description.ModelMetadata.Description;
                }
            }
        }
    }
}

