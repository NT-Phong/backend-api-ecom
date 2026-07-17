using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Force lowercase URLs
builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

// Add services from other layers
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);



// Add API specific services
builder.Services.AddHttpContextAccessor();
builder.Services.AddApiServices(builder.Configuration);

var timeout = 10;
var value = builder.Configuration.GetSection("ApplicationTimeout").Value;
if (value != null)
    timeout = int.Parse(value);

// Request timeout: 10 seconds or custom
builder.Services.AddRequestTimeouts(options =>
{
    options.DefaultPolicy = new Microsoft.AspNetCore.Http.Timeouts.RequestTimeoutPolicy
    {
        Timeout = TimeSpan.FromSeconds(timeout)
    };
});

// Add API versioning and Swagger
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(e => e.Value != null && e.Value.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
            );

        var firstErrorMessage = errors.Values.SelectMany(x => x).FirstOrDefault();

        var response = new
        {
            success = false,
            data = (object?)null,
            message = firstErrorMessage ?? "Dữ liệu không hợp lệ.",
            errorCode = "BAD_REQUEST",
            validationErrors = errors,
            details = firstErrorMessage,
            timestamp = DateTime.UtcNow
        };

        return new BadRequestObjectResult(response);
    };
});

// Add observability
builder.Services.AddSerilogLogging(builder.Configuration);

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 10485760; // 10MB
});

builder.Services.AddHttpClient();
var app = builder.Build();

// Handle forwarded headers from reverse proxy (Azure Application Gateway)
var forwardedHeaderOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
forwardedHeaderOptions.KnownIPNetworks.Clear();
forwardedHeaderOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedHeaderOptions);
// CORS must be before other middleware to handle preflight OPTIONS requests
app.UseCors("DefaultPolicy");
// Seed policies from Permissions constants into DB
await Ecom.Infrastructure.Seeding.PolicySeeder.SeedAsync(app.Services);

// Seed default roles (SystemAdmin, Admin, Manager, User) and assign policies
await Ecom.Infrastructure.Seeding.RoleSeeder.SeedAsync(app.Services);



// Initialize HttpContextEnricher
SerilogExtensions.InitializeHttpContextEnricher(app.Services);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var descriptions = app.Services.GetRequiredService<IApiVersionDescriptionProvider>().ApiVersionDescriptions;
        foreach (var description in descriptions.Reverse())
        {
            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                description.GroupName.ToUpperInvariant());
        }
    });
    app.UseReDoc(options =>
    {
        options.DocumentTitle = "Starter API Documentation";
        options.SpecUrl = "/swagger/v1/swagger.json";
        options.RoutePrefix = "redocs";
    });
}

// app.UseHttpsRedirection(); // Disabled for HTTP-only deployment


// Add custom middleware
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<StructuredLoggingMiddleware>();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<ProxyAuthorizationMiddleware>();

// Add standard middleware
app.UseRequestTimeouts();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
// Health check endpoints
app.MapHealthChecks("/healthz", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/livez", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/readyz", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

// Prometheus metrics endpoint
var telemetryEnabled = app.Configuration.GetValue<bool>("OpenTelemetry:Enabled");
if (telemetryEnabled)
{
    app.MapPrometheusScrapingEndpoint();
}
app.Run();
