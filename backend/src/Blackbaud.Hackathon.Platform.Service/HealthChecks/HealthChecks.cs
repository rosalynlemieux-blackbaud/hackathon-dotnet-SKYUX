using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Blackbaud.Hackathon.Platform.Shared.DataAccess;

namespace Blackbaud.Hackathon.Platform.Service.HealthChecks;

/// <summary>
/// Database health check to verify SQL Server connectivity
/// </summary>
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly HackathonDbContext _context;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(HackathonDbContext context, ILogger<DatabaseHealthCheck> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Try to execute a simple query
            await _context.Database.ExecuteSqlRawAsync("SELECT 1", cancellationToken);
            
            _logger.LogDebug("Database health check passed");
            
            return HealthCheckResult.Healthy("Database is responsive");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            
            return HealthCheckResult.Unhealthy(
                "Database is not responding",
                ex,
                new Dictionary<string, object>
                {
                    { "error", ex.Message }
                });
        }
    }
}

/// <summary>
/// Memory health check to monitor application memory usage
/// </summary>
public class MemoryHealthCheck : IHealthCheck
{
    private readonly ILogger<MemoryHealthCheck> _logger;
    private const long MaxMemoryBytes = 1_000_000_000; // 1 GB threshold

    public MemoryHealthCheck(ILogger<MemoryHealthCheck> logger)
    {
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var allocatedMemory = GC.GetTotalMemory(false);
        var memoryMB = allocatedMemory / 1_048_576; // Convert to MB

        var data = new Dictionary<string, object>
        {
            { "AllocatedMemoryMB", memoryMB },
            { "Gen0Collections", GC.CollectionCount(0) },
            { "Gen1Collections", GC.CollectionCount(1) },
            { "Gen2Collections", GC.CollectionCount(2) }
        };

        if (allocatedMemory < MaxMemoryBytes)
        {
            _logger.LogDebug("Memory health check passed: {MemoryMB}MB", memoryMB);
            return Task.FromResult(HealthCheckResult.Healthy($"Memory usage is {memoryMB}MB", data));
        }

        _logger.LogWarning("Memory health check degraded: {MemoryMB}MB", memoryMB);
        return Task.FromResult(HealthCheckResult.Degraded($"Memory usage is high: {memoryMB}MB", null, data));
    }
}

/// <summary>
/// External service health check (e.g., Blackbaud OAuth)
/// </summary>
public class ExternalServiceHealthCheck : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ExternalServiceHealthCheck> _logger;

    public ExternalServiceHealthCheck(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<ExternalServiceHealthCheck> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(5);

            // Check Blackbaud OAuth endpoint
            var oauthEndpoint = _configuration["BlackbaudAuth:AuthorizationEndpoint"];
            if (string.IsNullOrEmpty(oauthEndpoint))
            {
                return HealthCheckResult.Degraded("OAuth endpoint not configured");
            }

            var response = await client.GetAsync(oauthEndpoint, cancellationToken);
            
            if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                // 401 is expected without credentials, means service is reachable
                _logger.LogDebug("External service health check passed");
                return HealthCheckResult.Healthy("External services are reachable");
            }

            _logger.LogWarning("External service health check degraded: {StatusCode}", response.StatusCode);
            return HealthCheckResult.Degraded($"External service returned {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "External service health check failed");
            return HealthCheckResult.Unhealthy("External services are not reachable", ex);
        }
    }
}

/// <summary>
/// Custom health check response writer for detailed JSON output
/// </summary>
public static class HealthCheckResponseWriter
{
    public static Task WriteResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var result = new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow,
            duration = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds,
                exception = e.Value.Exception?.Message,
                data = e.Value.Data
            })
        };

        return context.Response.WriteAsJsonAsync(result);
    }
}
