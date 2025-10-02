using CatalogApi.Models;
using CatalogApi.Models.DTOs;
using CatalogApi.Models.Requests;
using CatalogApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CatalogApi.Handlers;

/// <summary>
/// Handler for search operations
/// </summary>
public static class SearchHandler
{
    /// <summary>
    /// Handle GET /search request
    /// </summary>
    /// <param name="q">Search query</param>
    /// <param name="kinds">Entity types to search for</param>
    /// <param name="limit">Maximum number of results</param>
    /// <param name="offset">Number of results to skip</param>
    /// <param name="sortBy">Field to sort by</param>
    /// <param name="sortOrder">Sort order</param>
    /// <param name="filters">Additional filters</param>
    /// <param name="knowledgeBaseService">Knowledge base service</param>
    /// <param name="cacheService">Cache service</param>
    /// <param name="observabilityService">Observability service</param>
    /// <param name="logger">Logger</param>
    /// <returns>Search results</returns>
    public static async Task<Results<Ok<Models.DTOs.SearchResponse>, BadRequest<ErrorResponse>>> Search(
        [FromQuery] string q,
        [FromQuery] string? kinds = null,
        [FromQuery] int limit = 50,
        [FromQuery] int offset = 0,
        [FromQuery] string sortBy = "relevance",
        [FromQuery] string sortOrder = "desc",
        [FromQuery] string? filters = null,
        IKnowledgeBaseService? knowledgeBaseService = null,
        ICacheService? cacheService = null,
        IObservabilityService? observabilityService = null,
        ILogger? logger = null)
    {
        observabilityService?.LogInformation("Processing search request: q={Query}, kinds={Kinds}", q, kinds);

        // Basic validation
        if (string.IsNullOrWhiteSpace(q))
        {
            return TypedResults.BadRequest(new ErrorResponse
            {
                Status = 400,
                Title = "Bad Request",
                Detail = "Query parameter 'q' is required.",
                ErrorCode = "SEARCH_001",
                Timestamp = DateTime.UtcNow
            });
        }

        // Parse kinds parameter
        var kindsList = new List<string>();
        if (!string.IsNullOrEmpty(kinds))
        {
            kindsList = kinds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(k => k.Trim())
                .ToList();
        }

        // Parse filters parameter
        var filtersDict = new Dictionary<string, object>();
        if (!string.IsNullOrEmpty(filters))
        {
            try
            {
                filtersDict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(filters) ?? new Dictionary<string, object>();
            }
            catch (Exception ex)
            {
                observabilityService?.LogWarning("Failed to parse filters parameter: {Error}", ex.Message);
                return TypedResults.BadRequest(new ErrorResponse
                {
                    Status = 400,
                    Title = "Bad Request",
                    Detail = "Invalid filters parameter format",
                    ErrorCode = "INVALID_FILTERS",
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        // Create search request
        var searchRequest = new SearchRequest
        {
            Query = q,
            Kinds = kindsList,
            Limit = Math.Min(limit, 1000), // Enforce maximum limit
            Offset = Math.Max(offset, 0), // Enforce minimum offset
            SortBy = sortBy,
            SortOrder = sortOrder,
            Filters = filtersDict
        };

        // Generate cache key
        var cacheKey = GenerateCacheKey(searchRequest);

        // Try to get from cache first
        var cachedResult = await cacheService?.GetOrCreateAsync(cacheKey, () => knowledgeBaseService?.SearchAsync(searchRequest) ?? Task.FromResult(new Models.DTOs.SearchResponse()), TimeSpan.FromMinutes(5));
        if (cachedResult != null)
        {
            observabilityService?.LogInformation("Search result returned from cache");
            return TypedResults.Ok(cachedResult);
        }

        // Execute search
        var result = await knowledgeBaseService?.SearchAsync(searchRequest) ?? new Models.DTOs.SearchResponse();

        observabilityService?.LogInformation("Search completed: {ResultCount} results in {QueryTime}ms", result.Results.Count, result.QueryTime.TotalMilliseconds);

        return TypedResults.Ok(result);
    }

    private static string GenerateCacheKey(SearchRequest request)
    {
        var keyParts = new List<string>
        {
            "search",
            request.Query,
            string.Join(",", request.Kinds.OrderBy(k => k)),
            request.Limit.ToString(),
            request.Offset.ToString(),
            request.SortBy,
            request.SortOrder
        };

        if (request.Filters.Any())
        {
            var filtersJson = System.Text.Json.JsonSerializer.Serialize(request.Filters.OrderBy(f => f.Key));
            keyParts.Add(filtersJson);
        }

        return string.Join(":", keyParts);
    }
}
