using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace CatalogApi.Middleware;

/// <summary>
/// Middleware for adding cache headers to responses
/// </summary>
public class CachingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CachingMiddleware> _logger;
    private readonly IDistributedCache _cache;

    public CachingMiddleware(RequestDelegate next, ILogger<CachingMiddleware> logger, IDistributedCache cache)
    {
        _next = next;
        _logger = logger;
        _cache = cache;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only cache GET requests
        if (context.Request.Method != "GET")
        {
            await _next(context);
            return;
        }

        // Generate cache key based on request path and query string
        var cacheKey = GenerateCacheKey(context.Request);
        
        // Try to get cached response
        var cachedResponse = await _cache.GetStringAsync(cacheKey);
        
        if (!string.IsNullOrEmpty(cachedResponse))
        {
            _logger.LogDebug("Cache hit for key: {CacheKey}", cacheKey);
            
            // Return cached response
            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json";
            context.Response.Headers.Add("X-Cache", "HIT");
            context.Response.Headers.Add("ETag", GenerateETag(cachedResponse));
            
            await context.Response.WriteAsync(cachedResponse);
            return;
        }

        _logger.LogDebug("Cache miss for key: {CacheKey}", cacheKey);

        // Store original response body stream
        var originalBodyStream = context.Response.Body;

        try
        {
            // Create a new memory stream for the response body
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            // Continue to the next middleware
            await _next(context);

            // Only cache successful responses
            if (context.Response.StatusCode == 200)
            {
                var responseBodyContent = await GetResponseBodyAsync(responseBody);
                
                // Generate ETag for the response
                var etag = GenerateETag(responseBodyContent);
                context.Response.Headers.Add("ETag", etag);
                context.Response.Headers.Add("X-Cache", "MISS");

                // Cache the response with appropriate TTL
                var cacheOptions = GetCacheOptions(context.Request.Path);
                if (cacheOptions != null)
                {
                    await _cache.SetStringAsync(cacheKey, responseBodyContent, cacheOptions);
                    _logger.LogDebug("Response cached with key: {CacheKey} and TTL: {TTL}", cacheKey, cacheOptions.AbsoluteExpirationRelativeToNow);
                }

                // Copy the response body back to the original stream
                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
            }
            else
            {
                // Copy the response body back to the original stream for non-200 responses
                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
            }
        }
        finally
        {
            // Ensure the original response body stream is restored
            context.Response.Body = originalBodyStream;
        }
    }

    private static string GenerateCacheKey(HttpRequest request)
    {
        var path = request.Path.Value ?? "";
        var queryString = request.QueryString.Value ?? "";
        return $"cache:{path}:{queryString}".ToLowerInvariant();
    }

    private static string GenerateETag(string content)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(content));
        return $"\"{Convert.ToBase64String(hash)[..16]}\"";
    }

    private static async Task<string> GetResponseBodyAsync(MemoryStream responseBody)
    {
        responseBody.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(responseBody);
        var body = await reader.ReadToEndAsync();
        responseBody.Seek(0, SeekOrigin.Begin);
        return body;
    }

    private static DistributedCacheEntryOptions? GetCacheOptions(PathString path)
    {
        return path.Value switch
        {
            var p when p.StartsWith("/api/v1/search") => new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) // Search results cache for 5 minutes
            },
            var p when p.StartsWith("/api/v1/collections") => new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30) // Collection details cache for 30 minutes
            },
            var p when p.StartsWith("/api/v1/types") => new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) // Type details cache for 1 hour
            },
            var p when p.StartsWith("/api/v1/graph") => new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15) // Graph data cache for 15 minutes
            },
            var p when p.StartsWith("/api/v1/diff") => new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24) // Diff data cache for 24 hours
            },
            _ => null // Don't cache other endpoints
        };
    }
}