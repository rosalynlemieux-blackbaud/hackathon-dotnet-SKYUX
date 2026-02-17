using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace Blackbaud.Hackathon.Platform.Service.Infrastructure;

/// <summary>
/// Caching service that provides both in-memory and distributed caching capabilities
/// </summary>
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default);
    T? Get<T>(string key);
    void Set<T>(string key, T value, TimeSpan? expiration = null);
    void Remove(string key);
}

public class CacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly IDistributedCache? _distributedCache;
    private readonly ILogger<CacheService> _logger;
    private readonly HashSet<string> _cacheKeys = new();
    private readonly object _lock = new();

    public CacheService(
        IMemoryCache memoryCache,
        ILogger<CacheService> logger,
        IDistributedCache? distributedCache = null)
    {
        _memoryCache = memoryCache;
        _distributedCache = distributedCache;
        _logger = logger;
    }

    /// <summary>
    /// Get value from cache asynchronously (checks distributed cache first, then memory cache)
    /// </summary>
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            // Try distributed cache first (if available)
            if (_distributedCache != null)
            {
                var distributedValue = await _distributedCache.GetStringAsync(key, cancellationToken);
                if (distributedValue != null)
                {
                    _logger.LogDebug("Cache hit (distributed): {Key}", key);
                    return JsonSerializer.Deserialize<T>(distributedValue);
                }
            }

            // Fall back to memory cache
            if (_memoryCache.TryGetValue(key, out T? memoryValue))
            {
                _logger.LogDebug("Cache hit (memory): {Key}", key);
                return memoryValue;
            }

            _logger.LogDebug("Cache miss: {Key}", key);
            return default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving from cache: {Key}", key);
            return default;
        }
    }

    /// <summary>
    /// Set value in cache asynchronously (stores in both distributed and memory cache)
    /// </summary>
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        try
        {
            expiration ??= TimeSpan.FromMinutes(30); // Default 30 minutes

            // Store in memory cache
            var memoryCacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };
            _memoryCache.Set(key, value, memoryCacheOptions);

            // Store in distributed cache (if available)
            if (_distributedCache != null)
            {
                var serializedValue = JsonSerializer.Serialize(value);
                var distributedCacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration
                };
                await _distributedCache.SetStringAsync(key, serializedValue, distributedCacheOptions, cancellationToken);
            }

            // Track key for bulk removal
            lock (_lock)
            {
                _cacheKeys.Add(key);
            }

            _logger.LogDebug("Cache set: {Key}, Expiration: {Expiration}", key, expiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache: {Key}", key);
        }
    }

    /// <summary>
    /// Remove value from cache asynchronously
    /// </summary>
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            _memoryCache.Remove(key);

            if (_distributedCache != null)
            {
                await _distributedCache.RemoveAsync(key, cancellationToken);
            }

            lock (_lock)
            {
                _cacheKeys.Remove(key);
            }

            _logger.LogDebug("Cache removed: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing from cache: {Key}", key);
        }
    }

    /// <summary>
    /// Remove all keys with a specific prefix (e.g., "hackathon:1:*")
    /// </summary>
    public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        try
        {
            List<string> keysToRemove;
            lock (_lock)
            {
                keysToRemove = _cacheKeys.Where(k => k.StartsWith(prefix)).ToList();
            }

            foreach (var key in keysToRemove)
            {
                await RemoveAsync(key, cancellationToken);
            }

            _logger.LogDebug("Cache cleared for prefix: {Prefix}, Keys removed: {Count}", prefix, keysToRemove.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cache with prefix: {Prefix}", prefix);
        }
    }

    /// <summary>
    /// Get value from memory cache synchronously
    /// </summary>
    public T? Get<T>(string key)
    {
        if (_memoryCache.TryGetValue(key, out T? value))
        {
            _logger.LogDebug("Memory cache hit: {Key}", key);
            return value;
        }

        _logger.LogDebug("Memory cache miss: {Key}", key);
        return default;
    }

    /// <summary>
    /// Set value in memory cache synchronously
    /// </summary>
    public void Set<T>(string key, T value, TimeSpan? expiration = null)
    {
        try
        {
            expiration ??= TimeSpan.FromMinutes(30);

            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };

            _memoryCache.Set(key, value, options);

            lock (_lock)
            {
                _cacheKeys.Add(key);
            }

            _logger.LogDebug("Memory cache set: {Key}, Expiration: {Expiration}", key, expiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting memory cache: {Key}", key);
        }
    }

    /// <summary>
    /// Remove value from memory cache synchronously
    /// </summary>
    public void Remove(string key)
    {
        _memoryCache.Remove(key);

        lock (_lock)
        {
            _cacheKeys.Remove(key);
        }

        _logger.LogDebug("Memory cache removed: {Key}", key);
    }
}

/// <summary>
/// Cache key builder for consistent key generation
/// </summary>
public static class CacheKeys
{
    public static string Hackathon(int id) => $"hackathon:{id}";
    public static string HackathonList() => "hackathons:list";
    public static string CurrentHackathon() => "hackathon:current";
    
    public static string Idea(int id) => $"idea:{id}";
    public static string IdeaList(int hackathonId) => $"hackathon:{hackathonId}:ideas";
    
    public static string Team(int id) => $"team:{id}";
    public static string TeamList(int hackathonId) => $"hackathon:{hackathonId}:teams";
    
    public static string Ratings(int ideaId) => $"idea:{ideaId}:ratings";
    public static string Comments(int ideaId) => $"idea:{ideaId}:comments";
    
    public static string Analytics(int hackathonId) => $"hackathon:{hackathonId}:analytics";
    public static string Leaderboard(int hackathonId) => $"hackathon:{hackathonId}:leaderboard";
}
