using CatalogApi.Models.DTOs;
using CatalogApi.Models.Requests;
using CatalogApi.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CatalogApi.Handlers;

/// <summary>
/// Handler for diff operations
/// </summary>
public class DiffHandler
{
    private readonly IKnowledgeBaseService _knowledgeBaseService;
    private readonly ICacheService _cacheService;
    private readonly IObservabilityService _observability;

    public DiffHandler(
        IKnowledgeBaseService knowledgeBaseService,
        ICacheService cacheService,
        IObservabilityService observability)
    {
        _knowledgeBaseService = knowledgeBaseService;
        _cacheService = cacheService;
        _observability = observability;
    }

    /// <summary>
    /// Handle GET /diff/type/{fqcn} request
    /// </summary>
    /// <param name="fqcn">Fully qualified class name</param>
    /// <param name="fromSha">Source commit SHA</param>
    /// <param name="toSha">Target commit SHA</param>
    /// <param name="includeFieldDetails">Whether to include field details</param>
    /// <param name="includeAttributeChanges">Whether to include attribute changes</param>
    /// <returns>Type diff</returns>
    public async Task<IResult> GetDiffAsync(
        [Required] string fqcn,
        [Required] string fromSha,
        [Required] string toSha,
        bool includeFieldDetails = true,
        bool includeAttributeChanges = true)
    {
        using var activity = _observability.StartActivity("DiffHandler.GetDiff");
        
        try
        {
            _observability.LogInformation("Processing get diff request", new Dictionary<string, object>
            {
                ["fqcn"] = fqcn,
                ["fromSha"] = fromSha,
                ["toSha"] = toSha,
                ["includeFieldDetails"] = includeFieldDetails,
                ["includeAttributeChanges"] = includeAttributeChanges
            });

            // Create diff request
            var diffRequest = new DiffRequest
            {
                FullyQualifiedName = fqcn,
                FromCommitSha = fromSha,
                ToCommitSha = toSha,
                IncludeFieldDetails = includeFieldDetails,
                IncludeAttributeChanges = includeAttributeChanges
            };

            // Validate request
            var validationContext = new ValidationContext(diffRequest);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(diffRequest, validationContext, validationResults, true))
            {
                var errors = validationResults.Select(vr => vr.ErrorMessage).ToList();
                _observability.LogWarning("Diff request validation failed", properties: new Dictionary<string, object>
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

            // Check if from and to SHA are the same
            if (fromSha == toSha)
            {
                _observability.LogWarning("Same commit SHA provided for diff");
                return Results.BadRequest(new ErrorResponse
                {
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    Title = "Bad Request",
                    Status = 400,
                    Detail = "From and to commit SHA cannot be the same",
                    ErrorCode = "SAME_COMMIT_SHA",
                    Timestamp = DateTime.UtcNow,
                    TraceId = _observability.GetTraceId()
                });
            }

            // Generate cache key
            var cacheKey = GenerateCacheKey(diffRequest);

            // Try to get from cache first
            var cachedResult = await _cacheService.GetAsync<TypeDiff>(cacheKey);
            if (cachedResult != null)
            {
                _observability.LogInformation("Type diff returned from cache");
                return Results.Ok(cachedResult);
            }

            // Execute diff query
            var result = await _knowledgeBaseService.GetDiffAsync(diffRequest);
            if (result == null)
            {
                _observability.LogWarning("Type diff not found", properties: new Dictionary<string, object>
                {
                    ["fqcn"] = fqcn,
                    ["fromSha"] = fromSha,
                    ["toSha"] = toSha
                });

                return Results.NotFound(new ErrorResponse
                {
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                    Title = "Not Found",
                    Status = 404,
                    Detail = $"Type diff for '{fqcn}' between commits '{fromSha}' and '{toSha}' not found",
                    ErrorCode = "TYPE_DIFF_NOT_FOUND",
                    Timestamp = DateTime.UtcNow,
                    TraceId = _observability.GetTraceId()
                });
            }

            // Cache the result
            var cacheTtl = TimeSpan.FromHours(24); // 24 hours TTL for type diffs
            await _cacheService.SetAsync(cacheKey, result, cacheTtl);

            _observability.LogInformation("Type diff retrieved successfully", new Dictionary<string, object>
            {
                ["fqcn"] = fqcn,
                ["fromSha"] = fromSha,
                ["toSha"] = toSha,
                ["addedFields"] = result.AddedFields.Count,
                ["removedFields"] = result.RemovedFields.Count,
                ["modifiedFields"] = result.ModifiedFields.Count,
                ["attributeChanges"] = result.AttributeChanges.Count
            });

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            _observability.LogError("Get diff request failed", ex);
            return Results.StatusCode(500);
        }
    }

    private string GenerateCacheKey(DiffRequest request)
    {
        var keyParts = new List<string>
        {
            "diff",
            request.FullyQualifiedName,
            request.FromCommitSha,
            request.ToCommitSha,
            request.IncludeFieldDetails.ToString(),
            request.IncludeAttributeChanges.ToString()
        };

        return string.Join(":", keyParts);
    }
}
