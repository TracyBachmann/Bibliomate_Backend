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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;

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

#region Database Configuration
// SQL Server via EF Core
builder.Services.AddDbContext<BiblioMateDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
#endregion

#region Controllers & JSON Options
builder.Services
    .AddControllers(opts =>
    {
        // Global authorization: require authenticated user by default
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
        apiOpts.InvalidModelStateResponseFactory = (ActionContext ctx) =>
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

        // Enable SignalR to pick token from query string
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
                // Verify security stamp
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

// -- Corrections locales pour la log des notifications --
builder.Services.AddScoped<INotificationLogCollection, NotificationLogCollection>();
builder.Services.AddScoped<NotificationService>();

// -- Hosted & Background Services --
builder.Services.AddScoped<IReservationCleanupService, ReservationCleanupService>();
builder.Services.AddScoped<LoanReminderService>();
builder.Services.AddHostedService<LoanReminderBackgroundService>();
#endregion

#region SignalR & Swagger
builder.Services.AddSignalR();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "BiblioMate API", Version = "v1" });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));

    // JWT Bearer in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description  = "JWT Authorization header using the Bearer scheme.",
        Type         = SecuritySchemeType.Http,
        Scheme       = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme 
            {
                Reference = new OpenApiReference 
                { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});
#endregion

var app = builder.Build();

#region Middleware Pipeline
// Swagger only in Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BiblioMate API v1");
        c.RoutePrefix = "swagger";
    });
    app.MapGet("/", () => Results.Redirect("/swagger"));
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
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
    });
#endregion

app.Run();