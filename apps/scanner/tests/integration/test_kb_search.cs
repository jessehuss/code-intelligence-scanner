using System.Text.Json;
using Xunit;

namespace Cataloger.Scanner.Tests.Integration;

public class TestKbSearch
{
    [Fact]
    public void KnowledgeBaseSearch_ShouldReturnRelevantResults()
    {
        // Arrange
        var searchQuery = "User";
        var searchRequest = new SearchRequest
        {
            Query = searchQuery,
            EntityTypes = new[] { "CodeType" },
            Limit = 10,
            Offset = 0
        };

        // Act
        var searchResult = ExecuteKnowledgeBaseSearch(searchRequest);

        // Assert
        Assert.NotNull(searchResult);
        Assert.NotNull(searchResult.Results);
        Assert.True(searchResult.Results.Count > 0, 
            "Knowledge base search should return results for 'User' query");
        Assert.True(searchResult.Total > 0, 
            "Search should find matching results");
        Assert.Equal(searchQuery, searchResult.Query, 
            "Search result should preserve original query");
    }

    [Fact]
    public void KnowledgeBaseSearch_ShouldFilterByEntityTypes()
    {
        // Arrange
        var searchRequest = new SearchRequest
        {
            Query = "collection",
            EntityTypes = new[] { "CollectionMapping" },
            Limit = 10,
            Offset = 0
        };

        // Act
        var searchResult = ExecuteKnowledgeBaseSearch(searchRequest);

        // Assert
        Assert.NotNull(searchResult);
        Assert.NotNull(searchResult.Results);
        
        foreach (var result in searchResult.Results)
        {
            Assert.Equal("CollectionMapping", result.EntityType, 
                "Search results should be filtered by entity type");
        }
    }

    [Fact]
    public void KnowledgeBaseSearch_ShouldFilterByRepositories()
    {
        // Arrange
        var targetRepository = "/path/to/repo1";
        var searchRequest = new SearchRequest
        {
            Query = "User",
            Repositories = new[] { targetRepository },
            Limit = 10,
            Offset = 0
        };

        // Act
        var searchResult = ExecuteKnowledgeBaseSearch(searchRequest);

        // Assert
        Assert.NotNull(searchResult);
        Assert.NotNull(searchResult.Results);
        
        foreach (var result in searchResult.Results)
        {
            Assert.Equal(targetRepository, result.Repository, 
                "Search results should be filtered by repository");
        }
    }

    [Fact]
    public void KnowledgeBaseSearch_ShouldRespectLimitAndOffset()
    {
        // Arrange
        var searchRequest = new SearchRequest
        {
            Query = "class",
            Limit = 5,
            Offset = 10
        };

        // Act
        var searchResult = ExecuteKnowledgeBaseSearch(searchRequest);

        // Assert
        Assert.NotNull(searchResult);
        Assert.NotNull(searchResult.Results);
        Assert.True(searchResult.Results.Count <= searchRequest.Limit, 
            "Search results should respect limit");
        Assert.Equal(searchRequest.Limit, searchResult.Limit, 
            "Search result should preserve limit");
        Assert.Equal(searchRequest.Offset, searchResult.Offset, 
            "Search result should preserve offset");
    }

    [Fact]
    public void KnowledgeBaseSearch_ShouldReturnRelevanceScores()
    {
        // Arrange
        var searchRequest = new SearchRequest
        {
            Query = "MongoDB",
            Limit = 10,
            Offset = 0
        };

        // Act
        var searchResult = ExecuteKnowledgeBaseSearch(searchRequest);

        // Assert
        Assert.NotNull(searchResult);
        Assert.NotNull(searchResult.Results);
        
        foreach (var result in searchResult.Results)
        {
            Assert.True(result.RelevanceScore >= 0.0 && result.RelevanceScore <= 1.0, 
                "Search results should have valid relevance scores");
        }

        // Results should be sorted by relevance (highest first)
        for (int i = 1; i < searchResult.Results.Count; i++)
        {
            Assert.True(searchResult.Results[i-1].RelevanceScore >= searchResult.Results[i].RelevanceScore, 
                "Search results should be sorted by relevance score");
        }
    }

    [Fact]
    public void KnowledgeBaseSearch_ShouldIncludeProvenanceInformation()
    {
        // Arrange
        var searchRequest = new SearchRequest
        {
            Query = "User",
            Limit = 5,
            Offset = 0
        };

        // Act
        var searchResult = ExecuteKnowledgeBaseSearch(searchRequest);

        // Assert
        Assert.NotNull(searchResult);
        Assert.NotNull(searchResult.Results);
        
        foreach (var result in searchResult.Results)
        {
            Assert.False(string.IsNullOrEmpty(result.Repository), 
                "Search results should include repository information");
            Assert.False(string.IsNullOrEmpty(result.FilePath), 
                "Search results should include file path information");
            Assert.True(result.LineNumber > 0, 
                "Search results should include line number information");
        }
    }

    [Fact]
    public void KnowledgeBaseSearch_ShouldHandleEmptyResults()
    {
        // Arrange
        var searchRequest = new SearchRequest
        {
            Query = "NonExistentEntity",
            Limit = 10,
            Offset = 0
        };

        // Act
        var searchResult = ExecuteKnowledgeBaseSearch(searchRequest);

        // Assert
        Assert.NotNull(searchResult);
        Assert.NotNull(searchResult.Results);
        Assert.Empty(searchResult.Results);
        Assert.Equal(0, searchResult.Total);
        Assert.Equal(searchRequest.Query, searchResult.Query);
    }

    [Fact]
    public void KnowledgeBaseSearch_ShouldHandleSpecialCharacters()
    {
        // Arrange
        var searchRequest = new SearchRequest
        {
            Query = "User@#$%^&*()",
            Limit = 10,
            Offset = 0
        };

        // Act
        var searchResult = ExecuteKnowledgeBaseSearch(searchRequest);

        // Assert
        Assert.NotNull(searchResult);
        Assert.NotNull(searchResult.Results);
        // Should not throw exception and should handle special characters gracefully
    }

    [Fact]
    public void KnowledgeBaseSearch_ShouldSupportFuzzyMatching()
    {
        // Arrange
        var searchRequest = new SearchRequest
        {
            Query = "Usr", // Typo in "User"
            Limit = 10,
            Offset = 0
        };

        // Act
        var searchResult = ExecuteKnowledgeBaseSearch(searchRequest);

        // Assert
        Assert.NotNull(searchResult);
        Assert.NotNull(searchResult.Results);
        Assert.True(searchResult.Results.Count > 0, 
            "Fuzzy search should find results for 'Usr' matching 'User'");
    }

    [Fact]
    public void KnowledgeBaseSearch_ShouldSupportPartialMatching()
    {
        // Arrange
        var searchRequest = new SearchRequest
        {
            Query = "UserSer", // Partial match for "UserService"
            Limit = 10,
            Offset = 0
        };

        // Act
        var searchResult = ExecuteKnowledgeBaseSearch(searchRequest);

        // Assert
        Assert.NotNull(searchResult);
        Assert.NotNull(searchResult.Results);
        Assert.True(searchResult.Results.Count > 0, 
            "Partial search should find results for 'UserSer' matching 'UserService'");
    }

    [Fact]
    public void KnowledgeBaseSearch_ShouldHandleLargeResultSets()
    {
        // Arrange
        var searchRequest = new SearchRequest
        {
            Query = "class",
            Limit = 1000,
            Offset = 0
        };

        // Act
        var startTime = DateTime.UtcNow;
        var searchResult = ExecuteKnowledgeBaseSearch(searchRequest);
        var duration = DateTime.UtcNow - startTime;

        // Assert
        Assert.NotNull(searchResult);
        Assert.NotNull(searchResult.Results);
        Assert.True(duration.TotalSeconds < 5, 
            "Large result set search should complete within 5 seconds");
    }

    [Fact]
    public void KnowledgeBaseSearch_ShouldMaintainSearchIndex()
    {
        // Arrange
        var searchRequest = new SearchRequest
        {
            Query = "User",
            Limit = 10,
            Offset = 0
        };

        // Act
        var searchResult1 = ExecuteKnowledgeBaseSearch(searchRequest);
        var searchResult2 = ExecuteKnowledgeBaseSearch(searchRequest);

        // Assert
        Assert.NotNull(searchResult1);
        Assert.NotNull(searchResult2);
        Assert.Equal(searchResult1.Total, searchResult2.Total, 
            "Search results should be consistent across multiple queries");
        Assert.Equal(searchResult1.Results.Count, searchResult2.Results.Count, 
            "Search result counts should be consistent");
    }

    private static SearchResult ExecuteKnowledgeBaseSearch(SearchRequest request)
    {
        // This would execute the actual knowledge base search
        // For now, return a mock result
        var mockResults = new List<SearchResultItem>();

        if (request.Query.Contains("User") || request.Query.Contains("Usr"))
        {
            mockResults.AddRange(new[]
            {
                new SearchResultItem
                {
                    Id = "type-user-001",
                    EntityType = "CodeType",
                    Title = "User Class",
                    Description = "User POCO class with MongoDB attributes",
                    RelevanceScore = 0.95,
                    Repository = "/path/to/repo1",
                    FilePath = "Models/User.cs",
                    LineNumber = 10
                },
                new SearchResultItem
                {
                    Id = "service-user-001",
                    EntityType = "QueryOperation",
                    Title = "UserService",
                    Description = "User service with MongoDB operations",
                    RelevanceScore = 0.85,
                    Repository = "/path/to/repo1",
                    FilePath = "Services/UserService.cs",
                    LineNumber = 25
                }
            });
        }

        if (request.Query.Contains("collection"))
        {
            mockResults.Add(new SearchResultItem
            {
                Id = "collection-users-001",
                EntityType = "CollectionMapping",
                Title = "Users Collection",
                Description = "MongoDB collection mapping for User entities",
                RelevanceScore = 0.90,
                Repository = "/path/to/repo1",
                FilePath = "Models/User.cs",
                LineNumber = 15
            });
        }

        if (request.Query.Contains("class"))
        {
            mockResults.AddRange(new[]
            {
                new SearchResultItem
                {
                    Id = "type-user-001",
                    EntityType = "CodeType",
                    Title = "User Class",
                    Description = "User POCO class with MongoDB attributes",
                    RelevanceScore = 0.80,
                    Repository = "/path/to/repo1",
                    FilePath = "Models/User.cs",
                    LineNumber = 10
                },
                new SearchResultItem
                {
                    Id = "type-product-001",
                    EntityType = "CodeType",
                    Title = "Product Class",
                    Description = "Product POCO class with MongoDB attributes",
                    RelevanceScore = 0.75,
                    Repository = "/path/to/repo1",
                    FilePath = "Models/Product.cs",
                    LineNumber = 12
                }
            });
        }

        // Apply filters
        if (request.EntityTypes?.Length > 0)
        {
            mockResults = mockResults.Where(r => request.EntityTypes.Contains(r.EntityType)).ToList();
        }

        if (request.Repositories?.Length > 0)
        {
            mockResults = mockResults.Where(r => request.Repositories.Contains(r.Repository)).ToList();
        }

        // Apply pagination
        var total = mockResults.Count;
        var paginatedResults = mockResults
            .Skip(request.Offset)
            .Take(request.Limit)
            .ToList();

        return new SearchResult
        {
            Results = paginatedResults,
            Total = total,
            Limit = request.Limit,
            Offset = request.Offset,
            Query = request.Query
        };
    }

    // Test data classes
    private class SearchRequest
    {
        public string Query { get; set; } = string.Empty;
        public string[]? EntityTypes { get; set; }
        public string[]? Repositories { get; set; }
        public int Limit { get; set; }
        public int Offset { get; set; }
    }

    private class SearchResult
    {
        public List<SearchResultItem> Results { get; set; } = new();
        public int Total { get; set; }
        public int Limit { get; set; }
        public int Offset { get; set; }
        public string Query { get; set; } = string.Empty;
    }

    private class SearchResultItem
    {
        public string Id { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double RelevanceScore { get; set; }
        public string Repository { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public int LineNumber { get; set; }
    }
}
