using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Blackbaud.Hackathon.Platform.Service.Infrastructure;

namespace Blackbaud.Hackathon.Platform.Service.Attributes;

/// <summary>
/// Attribute to enable response caching for controller actions
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class CachedAttribute : ActionFilterAttribute
{
    private readonly int _durationSeconds;
    private readonly string? _cacheKeyPrefix;

    public CachedAttribute(int durationSeconds = 300, string? cacheKeyPrefix = null)
    {
        _durationSeconds = durationSeconds;
        _cacheKeyPrefix = cacheKeyPrefix;
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var cacheService = context.HttpContext.RequestServices.GetService<ICacheService>();
        if (cacheService == null)
        {
            await next();
            return;
        }

        // Generate cache key from request
        var cacheKey = GenerateCacheKey(context);

        // Try to get cached response
        var cachedResponse = await cacheService.GetAsync<object>(cacheKey);
        if (cachedResponse != null)
        {
            context.Result = new OkObjectResult(cachedResponse);
            return;
        }

        // Execute action
        var executedContext = await next();

        // Cache successful responses
        if (executedContext.Result is OkObjectResult okResult && okResult.Value != null)
        {
            await cacheService.SetAsync(cacheKey, okResult.Value, TimeSpan.FromSeconds(_durationSeconds));
        }
    }

    private string GenerateCacheKey(ActionExecutingContext context)
    {
        var request = context.HttpContext.Request;
        var keyBuilder = new System.Text.StringBuilder();

        // Add prefix if specified
        if (!string.IsNullOrEmpty(_cacheKeyPrefix))
        {
            keyBuilder.Append($"{_cacheKeyPrefix}:");
        }

        // Add route info
        keyBuilder.Append($"{request.Path}");

        // Add query string
        if (request.QueryString.HasValue)
        {
            keyBuilder.Append($"{request.QueryString}");
        }

        // Add user identifier for user-specific caching
        var userId = context.HttpContext.User?.FindFirst("sub")?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            keyBuilder.Append($":user:{userId}");
        }

        return keyBuilder.ToString();
    }
}

/// <summary>
/// Middleware to automatically invalidate cache on data modifications
/// </summary>
public class CacheInvalidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CacheInvalidationMiddleware> _logger;

    public CacheInvalidationMiddleware(RequestDelegate next, ILogger<CacheInvalidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ICacheService cacheService)
    {
        await _next(context);

        // Invalidate cache on write operations (POST, PUT, DELETE)
        if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
        {
            var method = context.Request.Method;
            if (method == "POST" || method == "PUT" || method == "DELETE")
            {
                var path = context.Request.Path.Value?.ToLower() ?? "";
                
                // Invalidate related caches based on path
                if (path.Contains("/ideas"))
                {
                    await cacheService.RemoveByPrefixAsync("idea:");
                    await cacheService.RemoveByPrefixAsync("hackathon:");
                    _logger.LogDebug("Invalidated idea caches due to modification");
                }
                else if (path.Contains("/teams"))
                {
                    await cacheService.RemoveByPrefixAsync("team:");
                    await cacheService.RemoveByPrefixAsync("hackathon:");
                    _logger.LogDebug("Invalidated team caches due to modification");
                }
                else if (path.Contains("/ratings") || path.Contains("/comments"))
                {
                    await cacheService.RemoveByPrefixAsync("idea:");
                    await cacheService.RemoveByPrefixAsync("hackathon:");
                    _logger.LogDebug("Invalidated rating/comment caches due to modification");
                }
                else if (path.Contains("/hackathons"))
                {
                    await cacheService.RemoveByPrefixAsync("hackathon:");
                    _logger.LogDebug("Invalidated hackathon caches due to modification");
                }
            }
        }
    }
}
