using System.Reflection;
using System.Security.Claims;
using System.Text;
using BackendBiblioMate.Configuration;
using BackendBiblioMate.Data;
using BackendBiblioMate.Hubs;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Middlewares;
using BackendBiblioMate.Models.Enums;
using BackendBiblioMate.Services.Catalog;
using BackendBiblioMate.Services.Infrastructure.External;
using BackendBiblioMate.Services.Infrastructure.Logging;
using BackendBiblioMate.Services.Infrastructure.Security;
using BackendBiblioMate.Services.Loans;
using BackendBiblioMate.Services.Notifications;
using BackendBiblioMate.Services.Recommendations;
using BackendBiblioMate.Services.Reports;
using BackendBiblioMate.Services.Users;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using Prometheus;
using AspNetCoreRateLimit;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

#region CORS Configuration
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() 
    ?? new[] { "http://localhost:4200" };

builder.Services.AddCors(options =>
    options.AddPolicy("Default",
        policy => policy.WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()));
#endregion

#region API Versioning
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

#region Rate Limiting Configuration
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
#endregion

#region Database Configuration
builder.Services.AddDbContext<BiblioMateDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
#endregion

#region Controllers & JSON Options
builder.Services
    .AddControllers(opts =>
    {
        var policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
        opts.Filters.Add(new AuthorizeFilter(policy));
    })
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        opts.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    })
    .ConfigureApiBehaviorOptions(apiOpts =>
    {
        apiOpts.InvalidModelStateResponseFactory = ctx =>
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

#region Health Checks
builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<BiblioMateDbContext>(name: "sqlserver", tags: new[] { "db", "sql" })
    .AddSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")!,
        name: "sqlserver-raw", tags: new[] { "db", "sql" })
    .AddMongoDb(
        builder.Configuration.GetConnectionString("MongoDb")!,
        name: "mongodb", tags: new[] { "db", "mongo" });
#endregion

#region Authentication & JWT
var jwtSection = builder.Configuration.GetSection("Jwt");
var keyBytes   = Encoding.UTF8.GetBytes(jwtSection["Key"]!);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.RequireHttpsMetadata = false;
        opts.SaveToken = true;
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = jwtSection["Issuer"],
            ValidAudience            = jwtSection["Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(keyBytes),
            RoleClaimType            = ClaimTypes.Role
        };
        opts.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                if (ctx.Request.Path.StartsWithSegments("/hubs/notifications") &&
                    ctx.Request.Query.TryGetValue("access_token", out var token))
                {
                    ctx.Token = token;
                }
                return Task.CompletedTask;
            },
            OnTokenValidated = async ctx =>
            {
                var db     = ctx.HttpContext.RequestServices.GetRequiredService<BiblioMateDbContext>();
                var userId = int.Parse(ctx.Principal!.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var stamp  = ctx.Principal.FindFirst("stamp")?.Value;
                var user   = await db.Users.FindAsync(userId);
                if (user == null || user.SecurityStamp != stamp)
                    ctx.Fail("Invalid token: security stamp mismatch.");
            }
        };
    });
#endregion

#region MongoDB Configuration
builder.Services.Configure<MongoSettings>(
    builder.Configuration.GetSection("MongoDb"));
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoSettings>>().Value;
    return new MongoClient(settings.ConnectionString);
});
#endregion

#region Application Services Registration
// -- Core application services --
builder.Services.AddScoped<IAuthService, AuthService>();
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
// -- Infrastructure & external --
builder.Services.AddSingleton<EncryptionService>();
builder.Services.AddHttpClient<IGoogleBooksService, GoogleBooksService>();
builder.Services.AddScoped<IEmailService, SendGridEmailService>();
builder.Services.AddScoped<IMongoLogService, MongoLogService>();
// -- Local fixes for notification logging --
builder.Services.AddScoped<INotificationLogCollection, NotificationLogCollection>();
builder.Services.AddScoped<NotificationService>();
// -- Hosted & Background Services --
builder.Services.AddScoped<IReservationCleanupService, ReservationCleanupService>();
builder.Services.AddScoped<LoanReminderService>();
builder.Services.AddHostedService<LoanReminderBackgroundService>();
#endregion

#region SignalR, API Explorer & Swagger
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.DocInclusionPredicate((docName, apiDesc) =>
    {
        var versions = apiDesc.CustomAttributes()
            .OfType<ApiVersionAttribute>()
            .SelectMany(a => a.Versions);
        return versions.Any(v => $"v{v.ToString()}" == docName);
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
    c.EnableAnnotations();
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
        Description  = "JWT Authorization header using the Bearer scheme.",
        Type         = SecuritySchemeType.Http,
        Scheme       = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
#endregion

var app = builder.Build();

#region Middleware Pipeline
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var provider = scope.ServiceProvider.GetRequiredService<IApiVersionDescriptionProvider>();

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        foreach (var desc in provider.ApiVersionDescriptions)
        {
            c.SwaggerEndpoint($"/swagger/{desc.GroupName}/swagger.json",
                              $"BiblioMate API {desc.ApiVersion}");
        }
        c.RoutePrefix = "swagger";
    });

    app.MapGet("/", () => Results.Redirect("/swagger"))
       .ExcludeFromDescription();
}

// HTTP security headers
app.UseHsts();
app.Use(async (ctx, next) =>
{
    ctx.Response.Headers["X-Content-Type-Options"]  = "nosniff";
    ctx.Response.Headers["X-Frame-Options"]         = "DENY";
    ctx.Response.Headers["Referrer-Policy"]         = "no-referrer";
    ctx.Response.Headers["Content-Security-Policy"] = "default-src 'self'";
    await next();
});

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseIpRateLimiting();

// Prometheus metrics
app.UseMetricServer();
app.UseHttpMetrics();

app.UseHttpsRedirection();
app.UseCors("Default");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");

app.MapGet("/api/notifications/logs/user/{userId}",
    [Authorize(Roles = UserRoles.Librarian + "," + UserRoles.Admin)]
    async (int userId, INotificationLogService logService, CancellationToken ct) =>
    {
        var logs = await logService.GetByUserAsync(userId, ct);
        return Results.Ok(logs);
    })
   .ExcludeFromDescription();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (ctx, report) =>
    {
        ctx.Response.ContentType = "application/json";
        var result = new {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new {
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
