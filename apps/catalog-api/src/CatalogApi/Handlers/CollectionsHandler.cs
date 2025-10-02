using CatalogApi.Models;
using CatalogApi.Models.DTOs;
using CatalogApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CatalogApi.Handlers
{
    public static class CollectionsHandler
    {
        public static async Task<Results<Ok<CollectionDetail>, NotFound<ErrorResponse>>> GetCollectionByName(
            [FromRoute] string name,
            IKnowledgeBaseService? kbService = null,
            ICacheService? cacheService = null,
            IObservabilityService? observabilityService = null,
            ILogger? logger = null)
        {
            observabilityService?.LogInformation("Received request for collection: {CollectionName}", name);

            var cacheKey = $"collection:{name}";
            var collectionDetail = await cacheService?.GetOrCreateAsync(cacheKey, () => kbService?.GetCollectionDetailAsync(name) ?? Task.FromResult<CollectionDetail?>(null), TimeSpan.FromMinutes(30));

            if (collectionDetail == null)
            {
                return TypedResults.NotFound(new ErrorResponse
                {
                    Status = 404,
                    Title = "Not Found",
                    Detail = $"Collection '{name}' not found.",
                    ErrorCode = "COLLECTION_001",
                    Timestamp = DateTime.UtcNow
                });
            }

            return TypedResults.Ok(collectionDetail);
        }
    }
}
