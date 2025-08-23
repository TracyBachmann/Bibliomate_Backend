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
using BackendBiblioMate.Helpers;
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

// Configuration CORS améliorée
builder.Services.AddCors(options =>
{
    options.AddPolicy("Default", policy =>
        policy
            .WithOrigins(
                "http://localhost:4200", 
                "https://localhost:4200",
                "http://localhost:5000",
                "https://localhost:5001"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
    );
    
    // Politique pour le développement (plus permissive mais sans credentials)
    options.AddPolicy("Development", policy =>
        policy
            .WithOrigins(
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

builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

builder.Services.AddDbContext<BiblioMateDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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

builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<BiblioMateDbContext>(name: "sqlserver", tags: new[] { "db", "sql" })
    .AddSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")!,
        name: "sqlserver-raw",
        tags: new[] { "db", "sql" })
    .AddMongoDb(
        mongodbConnectionString: builder.Configuration.GetConnectionString("MongoDb")!,
        name: "mongodb",
        tags: new[] { "db", "mongo" });

var jwt = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwt["Key"]!);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.RequireHttpsMetadata = false;
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
            RoleClaimType            = ClaimTypes.Role
        };
        o.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                if (ctx.Request.Path.StartsWithSegments("/hubs/notifications") &&
                    ctx.Request.Query.TryGetValue("access_token", out var t))
                {
                    ctx.Token = t;
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

builder.Services.Configure<MongoSettings>(
    builder.Configuration.GetSection("MongoDb"));
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var s = sp.GetRequiredService<IOptions<MongoSettings>>().Value;
    return new MongoClient(s.ConnectionString);
});

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

builder.Services.AddScoped<INotificationLogCollection, NotificationLogCollection>();
builder.Services.AddScoped<IMongoLogService, MongoLogService>();
builder.Services.AddScoped<NotificationService>();

builder.Services.AddSingleton<EncryptionService>();
builder.Services.AddHttpClient<IGoogleBooksService, GoogleBooksService>();
builder.Services.AddScoped<IEmailService, SendGridEmailService>();

builder.Services.AddScoped<IReservationCleanupService, ReservationCleanupService>();
builder.Services.AddScoped<LoanReminderService>();
builder.Services.AddHostedService<LoanReminderBackgroundService>();

builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services
    .AddOptions<SwaggerGenOptions>()
    .Configure<IApiVersionDescriptionProvider>((options, provider) =>
    {
        foreach (var desc in provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(desc.GroupName, new OpenApiInfo { Title = $"BiblioMate API {desc.ApiVersion}", Version = desc.GroupName });
        }
        options.DocInclusionPredicate((docName, apiDesc) =>
        {
            if (apiDesc.GroupName != docName) return false;
            var path = apiDesc.RelativePath ?? "";
            return path.StartsWith($"api/{docName}/", StringComparison.OrdinalIgnoreCase);
        });
        options.OperationFilter<SwaggerDefaultValues>();
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
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
                new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
                Array.Empty<string>()
            }
        });
    });

var app = builder.Build();

var apiVersions = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        foreach (var desc in apiVersions.ApiVersionDescriptions)
        {
            c.SwaggerEndpoint($"/swagger/{desc.GroupName}/swagger.json", $"BiblioMate API {desc.ApiVersion}");
        }
        c.RoutePrefix = "swagger";
    });
    app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
}

app.UseHsts();

// Gestion des requêtes OPTIONS et CORS en premier
app.Use(async (ctx, next) =>
{
    if (ctx.Request.Method == HttpMethods.Options)
    {
        ctx.Response.StatusCode = StatusCodes.Status204NoContent;
        return;
    }
    await next();
});

// CORS doit être placé très tôt dans le pipeline
if (app.Environment.IsDevelopment())
{
    app.UseCors("Development");
}
else
{
    app.UseCors("Default");
}

// Headers de sécurité
app.Use(async (ctx, next) =>
{
    ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
    ctx.Response.Headers["X-Frame-Options"] = "DENY";
    ctx.Response.Headers["Referrer-Policy"] = "no-referrer";
    // Commenté temporairement pour tester
    // ctx.Response.Headers["Content-Security-Policy"] = "default-src 'self'";
    await next();
});

// Middleware de débogage pour les requêtes (optionnel, à retirer en production)
app.Use(async (ctx, next) =>
{
    Console.WriteLine($"Request: {ctx.Request.Method} {ctx.Request.Path}");
    Console.WriteLine($"Origin: {ctx.Request.Headers.Origin}");
    Console.WriteLine($"User-Agent: {ctx.Request.Headers.UserAgent}");
    Console.WriteLine($"Referer: {ctx.Request.Headers.Referer}");
    Console.WriteLine("---");
    await next();
});

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseIpRateLimiting();
app.UseMetricServer();
app.UseHttpMetrics();

app.UseHttpsRedirection();
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
        var result = new
        {
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

app.Run();