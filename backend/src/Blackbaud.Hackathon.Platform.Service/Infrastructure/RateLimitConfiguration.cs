using AspNetCoreRateLimit;
using Microsoft.Extensions.Options;

namespace Blackbaud.Hackathon.Platform.Service.Infrastructure;

/// <summary>
/// Rate limiting configuration and customization
/// </summary>
public class RateLimitConfiguration
{
    public static void Configure(IServiceCollection services, IConfiguration configuration)
    {
        // Load configuration
        services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));
        services.Configure<IpRateLimitPolicies>(configuration.GetSection("IpRateLimitPolicies"));

        // Inject counter and rules stores
        services.AddMemoryCache();
        services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
        services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
    }

    public static IpRateLimitOptions GetDefaultOptions()
    {
        return new IpRateLimitOptions
        {
            EnableEndpointRateLimiting = true,
            StackBlockedRequests = false,
            RealIpHeader = "X-Real-IP",
            ClientIdHeader = "X-ClientId",
            HttpStatusCode = 429,
            
            GeneralRules = new List<RateLimitRule>
            {
                new RateLimitRule
                {
                    Endpoint = "*",
                    Period = "1s",
                    Limit = 10
                },
                new RateLimitRule
                {
                    Endpoint = "*",
                    Period = "1m",
                    Limit = 100
                },
                new RateLimitRule
                {
                    Endpoint = "*",
                    Period = "1h",
                    Limit = 1000
                }
            },
            
            // More restrictive limits for authentication endpoints
            EndpointWhitelist = new List<string> { "*:/api/health" },
            
            ClientWhitelist = new List<string> { },
            
            QuotaExceededResponse = new QuotaExceededResponse
            {
                Content = "{{ \"error\": \"Rate limit exceeded. Please try again later.\" }}",
                ContentType = "application/json",
                StatusCode = 429
            }
        };
    }
}

/// <summary>
/// Custom rate limit configuration resolver
/// </summary>
public class CustomRateLimitConfiguration : RateLimitConfiguration, IRateLimitConfiguration
{
    private readonly IOptions<IpRateLimitOptions> _options;
    private readonly IOptions<IpRateLimitPolicies> _policies;

    public CustomRateLimitConfiguration(
        IOptions<IpRateLimitOptions> options,
        IOptions<IpRateLimitPolicies> policies)
    {
        _options = options;
        _policies = policies;
    }

    public IpRateLimitOptions IpRateLimitOptions => _options.Value;

    public void RegisterResolvers()
    {
        // Custom resolvers can be registered here
    }
}
