using Blackbaud.Hackathon.Platform.Service.Attributes;
using Blackbaud.Hackathon.Platform.Service.DataAccess;
using Blackbaud.Hackathon.Platform.Service.HealthChecks;
using Blackbaud.Hackathon.Platform.Service.Infrastructure;
using Blackbaud.Hackathon.Platform.Service.Middleware;
using Blackbaud.Hackathon.Platform.Shared.BusinessLogic;
using Blackbaud.Hackathon.Platform.Shared.DataAccess;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IO.Compression;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Database
builder.Services.AddDbContext<HackathonDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register HttpClient for API calls
builder.Services.AddHttpClient();

// Register application services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<IRatingService, RatingService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<DbSeeder>();

// Add SignalR for real-time notifications
builder.Services.AddSignalR();

//Performance Optimizations - Caching
builder.Services.AddMemoryCache();
builder.Services.AddDistributedMemoryCache(); // In production, use Redis: AddStackExchangeRedisCache
builder.Services.AddScoped<ICacheService, CacheService>();

// Performance Optimizations - Response Compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.SmallestSize;
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:4200" })
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Set to true in production
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ParticipantOnly", policy => policy.RequireRole("participant", "judge", "admin"));
    options.AddPolicy("JudgeOnly", policy => policy.RequireRole("judge", "admin"));
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("admin"));
});

// Production Hardening - Health Checks
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database")
    .AddCheck<MemoryHealthCheck>("memory")
    .AddCheck<ExternalServiceHealthCheck>("external_services");

// Add HTTP client factory for health checks
builder.Services.AddHttpClient();

// Configure HSTS for production
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});

var app = builder.Build();
Database Initialization and Seeding
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<HackathonDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();
        
        // Apply any pending migrations
        logger.LogInformation("Applying database migrations...");
        await context.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied successfully.");
        
        // Seed database if enabled in configuration
        var seedDatabase = builder.Configuration.GetValue<bool>("Database:SeedOnStartup", false);
        if (seedDatabase)
        {
            logger.LogInformation("Database seeding is enabled. Starting seed process...");
            var seeder = services.GetRequiredService<DbSeeder>();
// Performance middleware (order matters!)
app.UseResponseCompression();

app.UseHttpsRedirection();

app.UseCors("AllowAngularApp");

// Cache invalidation middleware
app.UseMiddleware<CacheInvalidationMiddleware>(
        {
            logger.LogInformation("Database seeding is disabled. Set 'Database:SeedOnStartup' to true to enable.");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database.");
        throw;
    }
}

// Production middleware for error handling
app.UseExceptionHandler("/error");
app.UseStatusCodePages();

// 
// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAngularApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Map health checks endpoint
app.MapHealthChecks("/health");

// Map SignalR NotificationHub
app.MapHub<Blackbaud.Hackathon.Platform.Service.Hubs.NotificationHub>("/hubs/notifications");

app.Run();
