namespace CatalogApi.Configuration;

/// <summary>
/// Configuration options for caching
/// </summary>
public class CacheConfiguration
{
    public const string SectionName = "CacheSettings";

    /// <summary>
    /// Default TTL for cached items in minutes
    /// </summary>
    public int DefaultTTLMinutes { get; set; } = 30;

    /// <summary>
    /// TTL for search results in minutes
    /// </summary>
    public int SearchTTLMinutes { get; set; } = 5;

    /// <summary>
    /// TTL for collection detail responses in minutes
    /// </summary>
    public int CollectionDetailTTLMinutes { get; set; } = 30;

    /// <summary>
    /// TTL for type detail responses in minutes
    /// </summary>
    public int TypeDetailTTLMinutes { get; set; } = 60;

    /// <summary>
    /// TTL for graph responses in minutes
    /// </summary>
    public int GraphTTLMinutes { get; set; } = 15;

    /// <summary>
    /// TTL for type diff responses in minutes
    /// </summary>
    public int TypeDiffTTLMinutes { get; set; } = 1440; // 24 hours

    /// <summary>
    /// Enable distributed caching
    /// </summary>
    public bool EnableDistributedCache { get; set; } = true;

    /// <summary>
    /// Enable in-memory caching as fallback
    /// </summary>
    public bool EnableInMemoryCache { get; set; } = true;

    /// <summary>
    /// Maximum cache size in MB
    /// </summary>
    public int MaxCacheSizeMB { get; set; } = 100;

    /// <summary>
    /// Cache key prefix
    /// </summary>
    public string KeyPrefix { get; set; } = "catalog_api";

    /// <summary>
    /// Enable cache compression
    /// </summary>
    public bool EnableCompression { get; set; } = false;

    /// <summary>
    /// Cache eviction policy
    /// </summary>
    public CacheEvictionPolicy EvictionPolicy { get; set; } = CacheEvictionPolicy.LRU;
}

/// <summary>
/// Cache eviction policies
/// </summary>
public enum CacheEvictionPolicy
{
    /// <summary>
    /// Least Recently Used
    /// </summary>
    LRU,

    /// <summary>
    /// Least Frequently Used
    /// </summary>
    LFU,

    /// <summary>
    /// First In First Out
    /// </summary>
    FIFO,

    /// <summary>
    /// Random replacement
    /// </summary>
    Random
}