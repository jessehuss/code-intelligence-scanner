using CatalogApi.Models.DTOs;
using CatalogApi.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CatalogApi.Handlers;

/// <summary>
/// Handler for type operations
/// </summary>
public class TypesHandler
{
    private readonly IKnowledgeBaseService _knowledgeBaseService;
    private readonly ICacheService _cacheService;
    private readonly IObservabilityService _observability;

    public TypesHandler(
        IKnowledgeBaseService knowledgeBaseService,
        ICacheService cacheService,
        IObservabilityService observability)
    {
        _knowledgeBaseService = knowledgeBaseService;
        _cacheService = cacheService;
        _observability = observability;
    }

    /// <summary>
    /// Handle GET /types/{fqcn} request
    /// </summary>
    /// <param name="fqcn">Fully qualified class name</param>
    /// <returns>Type details</returns>
    public async Task<IResult> GetTypeAsync([Required] string fqcn)
    {
        using var activity = _observability.StartActivity("TypesHandler.GetType");
        
        try
        {
            _observability.LogInformation("Processing get type request", new Dictionary<string, object>
            {
                ["fqcn"] = fqcn
            });

            // Validate input
            if (string.IsNullOrWhiteSpace(fqcn))
            {
                _observability.LogWarning("Invalid FQCN provided");
                return Results.BadRequest(new ErrorResponse
                {
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    Title = "Bad Request",
                    Status = 400,
                    Detail = "Fully qualified class name is required",
                    ErrorCode = "INVALID_FQCN",
                    Timestamp = DateTime.UtcNow,
                    TraceId = _observability.GetTraceId()
                });
            }

            // Generate cache key
            var cacheKey = $"type:{fqcn.ToLowerInvariant()}";

            // Try to get from cache first
            var cachedResult = await _cacheService.GetAsync<TypeDetail>(cacheKey);
            if (cachedResult != null)
            {
                _observability.LogInformation("Type details returned from cache");
                return Results.Ok(cachedResult);
            }

            // Get type details
            var result = await _knowledgeBaseService.GetTypeAsync(fqcn);
            if (result == null)
            {
                _observability.LogWarning("Type not found", properties: new Dictionary<string, object>
                {
                    ["fqcn"] = fqcn
                });

                return Results.NotFound(new ErrorResponse
                {
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                    Title = "Not Found",
                    Status = 404,
                    Detail = $"Type '{fqcn}' not found",
                    ErrorCode = "TYPE_NOT_FOUND",
                    Timestamp = DateTime.UtcNow,
                    TraceId = _observability.GetTraceId()
                });
            }

            // Cache the result
            var cacheTtl = TimeSpan.FromHours(1); // 1 hour TTL for type details
            await _cacheService.SetAsync(cacheKey, result, cacheTtl);

            _observability.LogInformation("Type details retrieved successfully", new Dictionary<string, object>
            {
                ["fqcn"] = fqcn,
                ["fieldCount"] = result.Fields.Count,
                ["bsonAttributesCount"] = result.BsonAttributes.Count,
                ["collectionMappingsCount"] = result.CollectionMappings.Count,
                ["totalChanges"] = result.ChangeSummary.TotalChanges,
                ["queryCount"] = result.UsageStats.QueryCount
            });

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            _observability.LogError("Get type request failed", ex);
            return Results.StatusCode(500);
        }
    }
}
