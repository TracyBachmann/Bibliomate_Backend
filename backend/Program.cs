using backend.Configuration;
using backend.Data;
using backend.Hubs;
using backend.Middlewares;
using backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1) CORS
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? new[] { "http://localhost:4200" };
builder.Services.AddCors(o => o.AddPolicy("default", p =>
    p.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

// 2) EF Core DbContext
builder.Services.AddDbContext<BiblioMateDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3) Controllers + global authorization + custom ModelState error responses
builder.Services
    .AddControllers(options =>
    {
        var policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
        options.Filters.Add(new AuthorizeFilter(policy));
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            // Collect only entries with errors
            var errors = context.ModelState
                .Where(kv => kv.Value!.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors
                        .Select(e => e.ErrorMessage)
                        .ToArray()
                );

            var result = new BadRequestObjectResult(new
            {
                error   = "ValidationError",
                details = errors
            });
            result.ContentTypes.Add("application/json");
            return result;
        };
    });

// 4) JWT Authentication & SignalR
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey   = jwtSettings["Key"]!;
builder.Services.AddAuthentication(opts =>
{
    opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opts.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(opts =>
{
    opts.RequireHttpsMetadata = false;
    opts.SaveToken            = true;
    opts.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer              = jwtSettings["Issuer"],
        ValidAudience            = jwtSettings["Audience"],
        IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
    opts.Events = new JwtBearerEvents
    {
        OnMessageReceived = ctx =>
        {
            if (ctx.HttpContext.Request.Path.StartsWithSegments("/hubs/notifications")
             && ctx.Request.Query.TryGetValue("access_token", out var token))
            {
                ctx.Token = token;
            }
            return Task.CompletedTask;
        }
    };
});

// 5) MongoDB
builder.Services.Configure<MongoSettings>(builder.Configuration.GetSection("MongoDb"));
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoSettings>>().Value;
    return new MongoClient(settings.ConnectionString);
});

// 6) Services & background
builder.Services.AddScoped<SendGridEmailService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<NotificationLogService>();
builder.Services.AddScoped<StockService>();
builder.Services.AddScoped<RecommendationService>();
builder.Services.AddScoped<ReservationCleanupService>();
builder.Services.AddScoped<HistoryService>();
builder.Services.AddHostedService<LoanReminderBackgroundService>();
builder.Services.AddScoped<UserActivityLogService>();
builder.Services.AddScoped<SearchActivityLogService>();
builder.Services.AddSingleton<EncryptionService>();
builder.Services.AddScoped<LoanReminderService>();

// 7) SignalR
builder.Services.AddSignalR();

// 8) Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "BiblioMate API", Version = "v1" });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.ApiKey,
        Scheme       = "Bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Type 'Bearer {token}'"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

var app = builder.Build();

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseCors("default");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");

app.MapGet("/api/notifications/logs/user/{userId}",
   [Authorize(Roles = "Librarian,Admin")]
   async (int userId, NotificationLogService svc) => Results.Ok(await svc.GetByUserAsync(userId)));

app.Run();