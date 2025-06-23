using backend.Configuration;
using backend.Data;
using backend.Hubs;
using backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --------------------------------------------------
// 1) CORS: load allowed origins from configuration
// --------------------------------------------------
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? new[] { "http://localhost:4200" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("default", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// --------------------------------------------------
// 2) Register EF Core DbContext
// --------------------------------------------------
builder.Services.AddDbContext<BiblioMateDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --------------------------------------------------
// 3) Add controllers with a global [Authorize] filter
// --------------------------------------------------
builder.Services.AddControllers(options =>
{
    var policy = new AuthorizationPolicyBuilder()
                     .RequireAuthenticatedUser()
                     .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});

builder.Services.AddEndpointsApiExplorer();

// --------------------------------------------------
// 4) Configure JWT authentication & SignalR token support
// --------------------------------------------------
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey   = jwtSettings["Key"]!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken            = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer              = jwtSettings["Issuer"],
        ValidAudience            = jwtSettings["Audience"],
        IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };

    // Allow JWT via query string for SignalR
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = ctx =>
        {
            var path = ctx.HttpContext.Request.Path;
            if (path.StartsWithSegments("/hubs/notifications") &&
                ctx.Request.Query.TryGetValue("access_token", out var token))
            {
                ctx.Token = token;
            }
            return Task.CompletedTask;
        }
    };
});

// --------------------------------------------------
// 5) Configure MongoDB client
// --------------------------------------------------
builder.Services.Configure<MongoSettings>(
    builder.Configuration.GetSection("MongoDb"));
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoSettings>>().Value;
    return new MongoClient(settings.ConnectionString);
});

// --------------------------------------------------
// 6) Register application services & background tasks
// --------------------------------------------------
builder.Services.AddScoped<SendGridEmailService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<NotificationLogService>();
builder.Services.AddScoped<StockService>();
builder.Services.AddScoped<RecommendationService>();
builder.Services.AddScoped<ReservationCleanupService>();
builder.Services.AddScoped<HistoryService>();
builder.Services.AddHostedService<LoanReminderBackgroundService>();

// --------------------------------------------------
// 7) Add SignalR
// --------------------------------------------------
builder.Services.AddSignalR();

// --------------------------------------------------
// 8) Configure Swagger with JWT support
// --------------------------------------------------
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "BiblioMate API",
        Version     = "v1",
        Description = "API documentation for the BiblioMate CDA project"
    });

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name        = "Authorization",
        Type        = SecuritySchemeType.ApiKey,
        Scheme      = "Bearer",
        BearerFormat= "JWT",
        In          = ParameterLocation.Header,
        Description = "Type 'Bearer {your_token}'"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// --------------------------------------------------
// HTTP request pipeline
// --------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Use the default CORS policy for both dev and prod
app.UseCors("default");

app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Map SignalR hub (protected by [Authorize] on the hub class)
app.MapHub<NotificationHub>("/hubs/notifications");

// Example of a role-protected endpoint
app.MapGet("/api/notifications/logs/user/{userId}",
    [Authorize(Roles = "Librarian,Admin")]
    async (int userId, NotificationLogService logService) =>
    {
        var logs = await logService.GetByUserAsync(userId);
        return Results.Ok(logs);
    });

app.Run();
