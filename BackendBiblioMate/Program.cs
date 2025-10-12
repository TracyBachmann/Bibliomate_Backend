using System.Reflection;
using System.Security.Claims;
using System.Text;
using BackendBiblioMate.Configuration;
using BackendBiblioMate.Data;
using BackendBiblioMate.Hubs;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Middlewares;
using BackendBiblioMate.Services.Catalog;
using BackendBiblioMate.Services.Infrastructure.External;
using BackendBiblioMate.Services.Infrastructure.Logging;
using BackendBiblioMate.Services.Infrastructure.Security;
using BackendBiblioMate.Services.Loans;
using BackendBiblioMate.Services.Notifications;
using BackendBiblioMate.Services.Recommendations;
using BackendBiblioMate.Services.Reports;
using BackendBiblioMate.Services.Users;
using BackendBiblioMate.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using Prometheus;
using AspNetCoreRateLimit;

// ============================================================================
// Program.cs
// Application bootstrap for BackendBiblioMate.
// - Configures data sources (SQL Server + optional MongoDB)
// - Sets up CORS, API versioning, rate limiting
// - Registers authentication/authorization (JWT + SignalR token handling)
// - Wires DI for domain services
// - Enables Swagger (versioned), health checks, Prometheus metrics
// - Adds global exception handling middleware and security headers
// - Applies EF Core migrations at startup
// ============================================================================

var builder = WebApplication.CreateBuilder(args);

#region Connection string helpers (SQL & Mongo)
/*
 * Resolve SQL Server / MongoDB connection strings from various conventional keys.
 * This enables flexibility across environments (local, CI/CD, containerized).
 */
string? GetSqlConnectionString()
{
    // Tries common keys in order; returns null if none present.
    return builder.Configuration.GetConnectionString("Default")
        ?? builder.Configuration.GetConnectionString("DefaultConnection")
        ?? builder.Configuration["ConnectionStrings:Default"]
        ?? builder.Configuration["ConnectionStrings:DefaultConnection"];
}

string? GetMongoConnectionString()
{
    // Single point of truth for Mongo connection string.
    return builder.Configuration.GetConnectionString("MongoDb")
        ?? builder.Configuration["MongoDb:ConnectionString"];
}
#endregion

#region CORS
/*
 * CORS policies:
 * - "Development": broader set of localhost origins for local frontends
 * - "Default": stricter list for non-development environments
 * Both policies allow credentials, any header, and any method.
 */
builder.Services.AddCors(options =>
{
    options.AddPolicy("Default", policy =>
        policy.WithOrigins(
                "http://localhost:4200",
                "https://localhost:4200",
                "http://localhost:5000",
                "https://localhost:5001"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
    );

    options.AddPolicy("Development", policy =>
        policy.WithOrigins(
                "http://localhost:4200",
                "https://localhost:4200",
                "http://localhost:3000",
                "http://localhost:5000",
                "https://localhost:5001",
                "https://localhost:7000",
                "https://localhost:7001"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
    );
});
#endregion

#region API Versioning
/*
 * URL, header, and query-string based API versioning.
 * - Default version: 1.0
 * - Explorer groups documented as 'vX' (e.g., v1)
 */
builder.Services.AddApiVersioning(opts =>
{
    opts.AssumeDefaultVersionWhenUnspecified = true;
    opts.DefaultApiVersion = new ApiVersion(1, 0);
    opts.ReportApiVersions = true;
    opts.ApiVersionReader = ApiVersionReader.Combine(
        new QueryStringApiVersionReader("api-version"),
        new HeaderApiVersionReader("X-API-Version"),
        new UrlSegmentApiVersionReader()
    );
});

builder.Services.AddVersionedApiExplorer(opts =>
{
    opts.GroupNameFormat = "'v'VVV";
    opts.SubstituteApiVersionInUrl = true;
});
#endregion

#region Rate limiting (IP-based)
/*
 * In-memory IP rate limiting with AspNetCoreRateLimit.
 * Stores policies and counters in MemoryCache.
 * NOTE: Policies and general rules must be provided in configuration (appsettings).
 */
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
#endregion

#region EF Core (SQL Server)
/*
 * SQL Server DbContext registration.
 * Fails fast if no connection string is configured.
 */
var sqlConnString = GetSqlConnectionString();
if (string.IsNullOrWhiteSpace(sqlConnString))
    throw new InvalidOperationException("No SQL connection string found (ConnectionStrings:Default or DefaultConnection).");

builder.Services.AddDbContext<BiblioMateDbContext>(o => o.UseSqlServer(sqlConnString));
#endregion

#region Controllers + JSON + ModelState shaping
/*
 * Global authorization policy: all endpoints require authentication by default.
 * Controllers can opt-out with [AllowAnonymous] or per-action [Authorize] attributes.
 *
 * JSON options:
 * - camelCase for properties and dictionary keys
 * - enums serialized as strings
 *
 * ModelState shaping:
 * - Returns a compact, consistent validation payload: { error, details }
 */
builder.Services
    .AddControllers(o =>
    {
        var policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
        o.Filters.Add(new AuthorizeFilter(policy));
    })
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        o.JsonSerializerOptions.DictionaryKeyPolicy  = System.Text.Json.JsonNamingPolicy.CamelCase;
        o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    })
    .ConfigureApiBehaviorOptions(opts =>
    {
        opts.InvalidModelStateResponseFactory = (ActionContext ctx) =>
        {
            var errors = ctx.ModelState
                .Where(kv => kv.Value!.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            var result = new BadRequestObjectResult(new { error = "ValidationError", details = errors });
            result.ContentTypes.Add("application/json");
            return result;
        };
    });
#endregion

#region HealthChecks
/*
 * Health checks for:
 * - EF Core DbContext (SQL)
 * - Raw SQL Server connection
 * - Optional MongoDB connection (only if configured)
 */
var healthChecks = builder.Services.AddHealthChecks()
    .AddDbContextCheck<BiblioMateDbContext>(name: "sqlserver-dbcontext", tags: new[] { "db", "sql" })
    .AddSqlServer(sqlConnString, name: "sqlserver-raw", tags: new[] { "db", "sql" });

var mongoConnString = GetMongoConnectionString();
if (!string.IsNullOrWhiteSpace(mongoConnString))
{
    healthChecks.AddMongoDb(mongoConnString, name: "mongodb", tags: new[] { "db", "mongo" });
}
#endregion

#region Authentication / JWT
/*
 * JWT bearer authentication with:
 * - Issuer/Audience/Key from configuration
 * - Name & Role claim mapping
 * - Security stamp verification on token validated
 * - SignalR support: token via 'access_token' query string for WebSocket connections
 *
 * IMPORTANT:
 * - o.MapInboundClaims = false keeps original claim names (e.g., "sub", "given_name").
 *   Ensure token generation aligns with claim types consumed by the app (NameIdentifier, Role).
 */
var jwt = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwt["Key"] ?? throw new InvalidOperationException("Missing Jwt:Key."));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.RequireHttpsMetadata = false; // set to true in production if only HTTPS is used
        o.SaveToken = true;

        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = jwt["Issuer"],
            ValidAudience            = jwt["Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(key),

            // Ensure role and name claims are correctly resolved in ClaimsPrincipal
            RoleClaimType            = ClaimTypes.Role,
            NameClaimType            = ClaimTypes.Name
        };

        // Keep inbound claim types as-is (do not remap to Microsoft legacy names).
        o.MapInboundClaims = false;

        o.Events = new JwtBearerEvents
        {
            // Allow SignalR clients to pass JWT via query string
            OnMessageReceived = ctx =>
            {
                if (ctx.Request.Path.StartsWithSegments("/hubs/notifications") &&
                    ctx.Request.Query.TryGetValue("access_token", out var t))
                {
                    ctx.Token = t;
                }
                return Task.CompletedTask;
            },

            // Validate security stamp on every request to support token invalidation
            OnTokenValidated = async ctx =>
            {
                var db     = ctx.HttpContext.RequestServices.GetRequiredService<BiblioMateDbContext>();
                var userId = int.Parse(ctx.Principal!.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var stamp  = ctx.Principal.FindFirst("stamp")?.Value;

                var user = await db.Users.FindAsync(userId);
                if (user == null || user.SecurityStamp != stamp)
                {
                    ctx.Fail("Invalid token: security stamp mismatch.");
                }
            }
        };
    });
#endregion

#region Mongo settings / client
/*
 * Mongo settings bound from configuration section "MongoDb".
 * Registers a singleton IMongoClient (no-op client if no connection string provided).
 */
builder.Services.Configure<MongoSettings>(builder.Configuration.GetSection("MongoDb"));
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var s = sp.GetRequiredService<IOptions<MongoSettings>>().Value;
    if (string.IsNullOrWhiteSpace(s.ConnectionString))
        return new MongoClient("mongodb://unused-host:27017"); // placeholder client when Mongo is disabled
    return new MongoClient(s.ConnectionString);
});
#endregion

#region Dependency Injection: domain services
/*
 * Registers application services and infrastructure components.
 * NOTE: NotificationService is registered both as interface and as concrete type to support
 *       scenarios where the concrete implementation is injected directly.
 */
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IHistoryService, HistoryService>();
builder.Services.AddScoped<IUserActivityLogService, UserActivityLogService>();
builder.Services.AddScoped<IAuthorService, AuthorService>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IEditorService, EditorService>();
builder.Services.AddScoped<IGenreService, GenreService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<INotificationLogService, NotificationLogService>();
builder.Services.AddScoped<IRecommendationService, RecommendationService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IShelfLevelService, ShelfLevelService>();
builder.Services.AddScoped<ILoanService, LoanService>();
builder.Services.AddScoped<ISearchActivityLogService, SearchActivityLogService>();
builder.Services.AddScoped<IShelfService, ShelfService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<IZoneService, ZoneService>();
builder.Services.AddScoped<ILocationService, LocationService>();

builder.Services.AddScoped<INotificationLogCollection, NotificationLogCollection>();
builder.Services.AddScoped<IMongoLogService, MongoLogService>();
builder.Services.AddScoped<NotificationService>(); // concrete registration (in addition to INotificationService)

builder.Services.AddSingleton<EncryptionService>();
builder.Services.AddHttpClient<IGoogleBooksService, GoogleBooksService>();

// Email provider selection based on configuration
var brevoApiKey = builder.Configuration["Brevo:ApiKey"];
if (!string.IsNullOrWhiteSpace(brevoApiKey))
{
    builder.Services.AddScoped<IEmailService, BrevoEmailService>();
}
else
{
    var smtpHost = builder.Configuration["Smtp:Host"];
    if (!string.IsNullOrWhiteSpace(smtpHost))
        builder.Services.AddScoped<IEmailService, SmtpEmailService>();
    else
        builder.Services.AddScoped<IEmailService, SendGridEmailService>();
}

// Background services & contextual infrastructure
builder.Services.AddScoped<IReservationCleanupService, ReservationCleanupService>();
builder.Services.AddScoped<LoanReminderService>();
builder.Services.AddHostedService<LoanReminderBackgroundService>();
builder.Services.AddHttpContextAccessor();
#endregion

#region Swagger / API explorer
/*
 * Swagger (OpenAPI) with API versioning integration:
 * - One document per API version (e.g., v1)
 * - Includes XML comments if generated (enable in .csproj)
 * - Adds JWT bearer security definition
 * - OperationFilter<SwaggerDefaultValues> allows default values for versioning
 */
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddOptions<Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions>()
    .Configure<IApiVersionDescriptionProvider>((options, provider) =>
    {
        foreach (var desc in provider.ApiVersionDescriptions)
            options.SwaggerDoc(desc.GroupName, new OpenApiInfo
            {
                Title   = $"BiblioMate API {desc.ApiVersion}",
                Version = desc.GroupName
            });

        // Only include endpoints whose route contains the version segment matching the doc name
        options.DocInclusionPredicate((docName, apiDesc) =>
        {
            if (apiDesc.GroupName != docName) return false;
            var path = apiDesc.RelativePath ?? "";
            return path.StartsWith($"api/{docName}/", StringComparison.OrdinalIgnoreCase);
        });

        options.OperationFilter<SwaggerDefaultValues>();

        // XML documentation (requires <GenerateDocumentationFile>true</GenerateDocumentationFile> in .csproj)
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
            options.IncludeXmlComments(xmlPath);

        options.EnableAnnotations();

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description  = "JWT Authorization header using the Bearer scheme.",
            Type         = SecuritySchemeType.Http,
            Scheme       = "bearer",
            BearerFormat = "JWT"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                Array.Empty<string>()
            }
        });
    });
#endregion

var app = builder.Build();

#region Swagger UI (accessible en production via nginx)
/*
 * Swagger UI accessible en production sans authentification
 * Les endpoints sont rendus accessibles anonymement via le middleware ci-dessous
 */
var apiVersions = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    foreach (var desc in apiVersions.ApiVersionDescriptions)
    {
        c.SwaggerEndpoint($"/swagger/{desc.GroupName}/swagger.json",
            $"BiblioMate API {desc.ApiVersion}");
    }
    c.RoutePrefix = "swagger";
    
    // Permet d'utiliser le JWT directement dans Swagger UI pour tester les endpoints
    c.ConfigObject.AdditionalItems["persistAuthorization"] = true;
});
#endregion

#region HTTP security and CORS
/*
 * Production HTTP hardening:
 * - HSTS and HTTPS redirection outside Development
 *
 * CORS:
 * - Selects "Development" policy in dev, "Default" otherwise
 *
 * Additional security headers:
 * - X-Content-Type-Options, X-Frame-Options, Referrer-Policy
 *   (CSP could be added if/when static content is served)
 */
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    // app.UseHttpsRedirection();
}

if (app.Environment.IsDevelopment())
    app.UseCors("Development");
else
    app.UseCors("Default");

// Minimal security headers suitable for API responses
app.Use(async (ctx, next) =>
{
    ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
    ctx.Response.Headers["X-Frame-Options"] = "DENY";
    ctx.Response.Headers["Referrer-Policy"] = "no-referrer";
    await next();
});
#endregion

#region Observability / error handling
/*
 * Global exception handling:
 * - Uniform JSON error payloads via ExceptionHandlingMiddleware
 *
 * Prometheus metrics:
 * - /metrics endpoint via UseMetricServer
 * - HTTP metrics middleware
 *
 * IP rate limiting:
 * - Applies rate limits before MVC
 */
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseIpRateLimiting();

app.UseMetricServer();
app.UseHttpMetrics();
#endregion

#region Swagger anonymous access middleware
/*
 * IMPORTANT : Ce middleware doit être AVANT UseAuthentication()
 * Il court-circuite l'authentification pour les endpoints Swagger uniquement
 */
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value?.ToLower() ?? "";
    
    // Autoriser l'accès anonyme à Swagger et aux documents OpenAPI
    if (path.StartsWith("/swagger") || path.Contains("swagger.json"))
    {
        // Skip authentication/authorization pour Swagger
        var endpoint = new RouteEndpoint(
            requestDelegate: async (ctx) => await next(),
            routePattern: RoutePatternFactory.Pattern("/swagger"),
            order: 0,
            metadata: new EndpointMetadataCollection(new AllowAnonymousAttribute()),
            displayName: "Swagger"
        );
        
        context.SetEndpoint(endpoint);
    }
    
    await next();
});
#endregion

#region Authentication / Authorization
/*
 * Authentication must precede Authorization.
 * With a global AuthorizeFilter on controllers, endpoints require auth by default.
 */
app.UseAuthentication();
app.UseAuthorization();
#endregion

#region EF Core migrations at startup
/*
 * Applies pending EF Core migrations automatically during startup.
 * Runs in a scoped service provider to resolve DbContext safely.
 */
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BiblioMateDbContext>();
    await db.Database.MigrateAsync();
}
#endregion

#region Endpoint mapping
/*
 * Controllers and SignalR hubs.
 * Health checks at /health with concise JSON payload.
 */
app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");

// Endpoint racine qui retourne les infos de l'API
app.MapGet("/", () => Results.Json(new 
{ 
    service = "BiblioMate API",
    status = "running",
    version = "1.0",
    documentation = "/swagger",
    health = "/health"
}))
.AllowAnonymous()
.ExcludeFromDescription();

// Health check endpoint (anonymous for external probes)
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (ctx, report) =>
    {
        ctx.Response.ContentType = "application/json";
        var result = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name     = e.Key,
                status   = e.Value.Status.ToString(),
                error    = e.Value.Exception?.Message,
                duration = e.Value.Duration.ToString()
            })
        };
        await ctx.Response.WriteAsJsonAsync(result);
    }
})
.AllowAnonymous();
#endregion

app.Run();

