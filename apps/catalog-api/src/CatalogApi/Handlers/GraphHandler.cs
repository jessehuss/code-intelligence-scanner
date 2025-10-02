using CatalogApi.Models.DTOs;
using CatalogApi.Models.Requests;
using CatalogApi.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CatalogApi.Handlers;

/// <summary>
/// Handler for graph operations
/// </summary>
public class GraphHandler
{
    private readonly IKnowledgeBaseService _knowledgeBaseService;
    private readonly ICacheService _cacheService;
    private readonly IObservabilityService _observability;

    public GraphHandler(
        IKnowledgeBaseService knowledgeBaseService,
        ICacheService cacheService,
        IObservabilityService observability)
    {
        _knowledgeBaseService = knowledgeBaseService;
        _cacheService = cacheService;
        _observability = observability;
    }

    /// <summary>
    /// Handle GET /graph request
    /// </summary>
    /// <param name="node">Node to start traversal from</param>
    /// <param name="depth">Maximum depth for traversal</param>
    /// <param name="edgeKinds">Edge types to include</param>
    /// <param name="maxNodes">Maximum number of nodes to return</param>
    /// <param name="includeProperties">Whether to include node properties</param>
    /// <returns>Graph data</returns>
    public async Task<IResult> GetGraphAsync(
        [Required] string node,
        int depth = 2,
        string? edgeKinds = null,
        int maxNodes = 100,
        bool includeProperties = false)
    {
        using var activity = _observability.StartActivity("GraphHandler.GetGraph");
        
        try
        {
            _observability.LogInformation("Processing get graph request", new Dictionary<string, object>
            {
                ["node"] = node,
                ["depth"] = depth,
                ["maxNodes"] = maxNodes,
                ["includeProperties"] = includeProperties
            });

            // Parse edge kinds parameter
            var edgeKindsList = new List<string>();
            if (!string.IsNullOrEmpty(edgeKinds))
            {
                edgeKindsList = edgeKinds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(k => k.Trim())
                    .ToList();
            }

            // Create graph request
            var graphRequest = new GraphRequest
            {
                Node = node,
                Depth = Math.Min(depth, 5), // Enforce maximum depth
                EdgeKinds = edgeKindsList,
                MaxNodes = Math.Max(10, Math.Min(maxNodes, 1000)), // Enforce min/max nodes
                IncludeProperties = includeProperties
            };

            // Validate request
            var validationContext = new ValidationContext(graphRequest);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(graphRequest, validationContext, validationResults, true))
            {
                var errors = validationResults.Select(vr => vr.ErrorMessage).ToList();
                _observability.LogWarning("Graph request validation failed", properties: new Dictionary<string, object>
                {
                    ["errors"] = errors
                });

                return Results.BadRequest(new ErrorResponse
                {
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    Title = "Validation Error",
                    Status = 400,
                    Detail = string.Join("; ", errors),
                    ErrorCode = "VALIDATION_FAILED",
                    Timestamp = DateTime.UtcNow,
                    TraceId = _observability.GetTraceId()
                });
            }

            // Generate cache key
            var cacheKey = GenerateCacheKey(graphRequest);

            // Try to get from cache first
            var cachedResult = await _cacheService.GetAsync<GraphResponse>(cacheKey);
            if (cachedResult != null)
            {
                _observability.LogInformation("Graph data returned from cache");
                return Results.Ok(cachedResult);
            }

            // Execute graph query
            var result = await _knowledgeBaseService.GetGraphAsync(graphRequest);

            // Cache the result
            var cacheTtl = TimeSpan.FromMinutes(15); // 15 minutes TTL for graph data
            await _cacheService.SetAsync(cacheKey, result, cacheTtl);

            _observability.LogInformation("Graph data retrieved successfully", new Dictionary<string, object>
            {
                ["node"] = node,
                ["totalNodes"] = result.TotalNodes,
                ["totalEdges"] = result.TotalEdges,
                ["queryTime"] = result.QueryTime.TotalMilliseconds
            });

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            _observability.LogError("Get graph request failed", ex);
            return Results.StatusCode(500);
        }
    }

    private string GenerateCacheKey(GraphRequest request)
    {
        var keyParts = new List<string>
        {
            "graph",
            request.Node,
            request.Depth.ToString(),
            string.Join(",", request.EdgeKinds.OrderBy(k => k)),
            request.MaxNodes.ToString(),
            request.IncludeProperties.ToString()
        };

        return string.Join(":", keyParts);
    }
}
