using StackExchange.Redis;
using System.Text.Json;

namespace CatalogApi.Services;

/// <summary>
/// Redis-based cache service implementation
/// </summary>
public class CacheService : ICacheService
{
    private readonly IDatabase _database;
    private readonly IObservabilityService _observability;
    private readonly JsonSerializerOptions _jsonOptions;

    public CacheService(IDatabase database, IObservabilityService observability)
    {
        _database = database;
        _observability = observability;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        using var activity = _observability.StartActivity("CacheService.Get");
        
        try
        {
            _observability.LogDebug("Getting value from cache", new Dictionary<string, object>
            {
                ["key"] = key
            });

            var value = await _database.StringGetAsync(key);
            
            if (!value.HasValue)
            {
                _observability.LogDebug("Cache miss", new Dictionary<string, object>
                {
                    ["key"] = key
                });
                return null;
            }

            var result = JsonSerializer.Deserialize<T>(value!, _jsonOptions);
            
            _observability.LogDebug("Cache hit", new Dictionary<string, object>
            {
                ["key"] = key
            });
            
            return result;
        }
        catch (Exception ex)
        {
            _observability.LogError("Failed to get value from cache", ex, new Dictionary<string, object>
            {
                ["key"] = key
            });
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan ttl) where T : class
    {
        using var activity = _observability.StartActivity("CacheService.Set");
        
        try
        {
            _observability.LogDebug("Setting value in cache", new Dictionary<string, object>
            {
                ["key"] = key,
                ["ttl"] = ttl.TotalSeconds
            });

            var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
            await _database.StringSetAsync(key, serializedValue, ttl);
            
            _observability.LogDebug("Value set in cache");
        }
        catch (Exception ex)
        {
            _observability.LogError("Failed to set value in cache", ex, new Dictionary<string, object>
            {
                ["key"] = key
            });
        }
    }

    public async Task RemoveAsync(string key)
    {
        using var activity = _observability.StartActivity("CacheService.Remove");
        
        try
        {
            _observability.LogDebug("Removing value from cache", new Dictionary<string, object>
            {
                ["key"] = key
            });

            await _database.KeyDeleteAsync(key);
            
            _observability.LogDebug("Value removed from cache");
        }
        catch (Exception ex)
        {
            _observability.LogError("Failed to remove value from cache", ex, new Dictionary<string, object>
            {
                ["key"] = key
            });
        }
    }

    public async Task<bool> ExistsAsync(string key)
    {
        using var activity = _observability.StartActivity("CacheService.Exists");
        
        try
        {
            _observability.LogDebug("Checking if key exists in cache", new Dictionary<string, object>
            {
                ["key"] = key
            });

            var exists = await _database.KeyExistsAsync(key);
            
            _observability.LogDebug("Key existence checked", new Dictionary<string, object>
            {
                ["key"] = key,
                ["exists"] = exists
            });
            
            return exists;
        }
        catch (Exception ex)
        {
            _observability.LogError("Failed to check key existence", ex, new Dictionary<string, object>
            {
                ["key"] = key
            });
            return false;
        }
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan ttl) where T : class
    {
        using var activity = _observability.StartActivity("CacheService.GetOrSet");
        
        try
        {
            _observability.LogDebug("Getting or setting value in cache", new Dictionary<string, object>
            {
                ["key"] = key
            });

            // Try to get from cache first
            var cachedValue = await GetAsync<T>(key);
            if (cachedValue != null)
            {
                return cachedValue;
            }

            // Not in cache, create new value
            var newValue = await factory();
            
            // Set in cache
            await SetAsync(key, newValue, ttl);
            
            _observability.LogDebug("Value created and cached");
            return newValue;
        }
        catch (Exception ex)
        {
            _observability.LogError("Failed to get or set value in cache", ex, new Dictionary<string, object>
            {
                ["key"] = key
            });
            throw;
        }
    }

    public async Task ClearAsync()
    {
        using var activity = _observability.StartActivity("CacheService.Clear");
        
        try
        {
            _observability.LogInformation("Clearing all cache entries");

            var server = _database.Multiplexer.GetServer(_database.Multiplexer.GetEndPoints().First());
            await server.FlushDatabaseAsync(_database.Database);
            
            _observability.LogInformation("Cache cleared");
        }
        catch (Exception ex)
        {
            _observability.LogError("Failed to clear cache", ex);
        }
    }

    public async Task<CacheStatistics> GetStatisticsAsync()
    {
        using var activity = _observability.StartActivity("CacheService.GetStatistics");
        
        try
        {
            _observability.LogDebug("Getting cache statistics");

            var server = _database.Multiplexer.GetServer(_database.Multiplexer.GetEndPoints().First());
            var info = await server.InfoAsync("stats");
            
            var stats = new CacheStatistics();
            
            foreach (var section in info)
            {
                foreach (var item in section)
                {
                    switch (item.Key)
                    {
                        case "keyspace_hits":
                            if (long.TryParse(item.Value, out var hits))
                                stats.HitCount = hits;
                            break;
                        case "keyspace_misses":
                            if (long.TryParse(item.Value, out var misses))
                                stats.MissCount = misses;
                            break;
                    }
                }
            }

            // Get total keys
            var keys = await _database.Multiplexer.GetDatabase().ExecuteAsync("DBSIZE");
            if (keys.Type == ResultType.Integer)
            {
                stats.TotalKeys = (long)keys;
            }

            _observability.LogDebug("Cache statistics retrieved", new Dictionary<string, object>
            {
                ["hitCount"] = stats.HitCount,
                ["missCount"] = stats.MissCount,
                ["totalKeys"] = stats.TotalKeys,
                ["hitRatio"] = stats.HitRatio
            });

            return stats;
        }
        catch (Exception ex)
        {
            _observability.LogError("Failed to get cache statistics", ex);
            return new CacheStatistics();
        }
    }
}
