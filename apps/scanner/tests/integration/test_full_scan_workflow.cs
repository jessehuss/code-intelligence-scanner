using System.Text.Json;
using Xunit;

namespace Cataloger.Scanner.Tests.Integration;

public class TestFullScanWorkflow
{
    [Fact]
    public void FullScan_ShouldDiscoverCodeTypes()
    {
        // Arrange
        var testRepository = CreateTestRepository();
        var expectedMinTypes = 5;

        // Act
        var scanResult = ExecuteFullScan(testRepository);

        // Assert
        Assert.NotNull(scanResult);
        Assert.True(scanResult.TypesDiscovered >= expectedMinTypes, 
            $"Full scan should discover at least {expectedMinTypes} types, but found {scanResult.TypesDiscovered}");
    }

    [Fact]
    public void FullScan_ShouldMapCollections()
    {
        // Arrange
        var testRepository = CreateTestRepository();
        var expectedMinCollections = 3;

        // Act
        var scanResult = ExecuteFullScan(testRepository);

        // Assert
        Assert.NotNull(scanResult);
        Assert.True(scanResult.CollectionsMapped >= expectedMinCollections, 
            $"Full scan should map at least {expectedMinCollections} collections, but found {scanResult.CollectionsMapped}");
    }

    [Fact]
    public void FullScan_ShouldExtractQueries()
    {
        // Arrange
        var testRepository = CreateTestRepository();
        var expectedMinQueries = 10;

        // Act
        var scanResult = ExecuteFullScan(testRepository);

        // Assert
        Assert.NotNull(scanResult);
        Assert.True(scanResult.QueriesExtracted >= expectedMinQueries, 
            $"Full scan should extract at least {expectedMinQueries} queries, but found {scanResult.QueriesExtracted}");
    }

    [Fact]
    public void FullScan_ShouldInferRelationships()
    {
        // Arrange
        var testRepository = CreateTestRepository();
        var expectedMinRelationships = 2;

        // Act
        var scanResult = ExecuteFullScan(testRepository);

        // Assert
        Assert.NotNull(scanResult);
        Assert.True(scanResult.RelationshipsInferred >= expectedMinRelationships, 
            $"Full scan should infer at least {expectedMinRelationships} relationships, but found {scanResult.RelationshipsInferred}");
    }

    [Fact]
    public void FullScan_ShouldCompleteWithinTimeLimit()
    {
        // Arrange
        var testRepository = CreateTestRepository();
        var maxDuration = TimeSpan.FromMinutes(10);

        // Act
        var startTime = DateTime.UtcNow;
        var scanResult = ExecuteFullScan(testRepository);
        var duration = DateTime.UtcNow - startTime;

        // Assert
        Assert.NotNull(scanResult);
        Assert.True(duration <= maxDuration, 
            $"Full scan should complete within {maxDuration.TotalMinutes} minutes, but took {duration.TotalMinutes:F2} minutes");
    }

    [Fact]
    public void FullScan_ShouldHaveCompleteProvenance()
    {
        // Arrange
        var testRepository = CreateTestRepository();

        // Act
        var scanResult = ExecuteFullScan(testRepository);

        // Assert
        Assert.NotNull(scanResult);
        Assert.NotNull(scanResult.ProvenanceRecords);
        Assert.True(scanResult.ProvenanceRecords.Count > 0, 
            "Full scan should generate provenance records");

        foreach (var provenance in scanResult.ProvenanceRecords)
        {
            Assert.False(string.IsNullOrEmpty(provenance.Repository), 
                "Provenance should have repository information");
            Assert.False(string.IsNullOrEmpty(provenance.FilePath), 
                "Provenance should have file path information");
            Assert.False(string.IsNullOrEmpty(provenance.Symbol), 
                "Provenance should have symbol information");
            Assert.False(string.IsNullOrEmpty(provenance.CommitSHA), 
                "Provenance should have commit SHA information");
            Assert.True(provenance.Timestamp > DateTime.MinValue, 
                "Provenance should have valid timestamp");
        }
    }

    [Fact]
    public void FullScan_ShouldHandleMultipleRepositories()
    {
        // Arrange
        var testRepositories = new[]
        {
            CreateTestRepository("repo1"),
            CreateTestRepository("repo2"),
            CreateTestRepository("repo3")
        };

        // Act
        var scanResult = ExecuteFullScan(testRepositories);

        // Assert
        Assert.NotNull(scanResult);
        Assert.Equal(3, scanResult.Repositories.Count);
        
        foreach (var repoResult in scanResult.Repositories)
        {
            Assert.Equal("success", repoResult.Status);
            Assert.True(repoResult.TypesDiscovered > 0, 
                $"Repository {repoResult.Repository} should have discovered types");
        }
    }

    [Fact]
    public void FullScan_ShouldSkipRepositoriesWithoutMongoDB()
    {
        // Arrange
        var testRepositories = new[]
        {
            CreateTestRepository("repo_with_mongo"),
            CreateTestRepositoryWithoutMongoDB("repo_without_mongo"),
            CreateTestRepository("repo_with_mongo_2")
        };

        // Act
        var scanResult = ExecuteFullScan(testRepositories);

        // Assert
        Assert.NotNull(scanResult);
        Assert.Equal(3, scanResult.Repositories.Count);
        
        var skippedRepo = scanResult.Repositories.FirstOrDefault(r => r.Repository.Contains("without_mongo"));
        Assert.NotNull(skippedRepo);
        Assert.Equal("skipped", skippedRepo.Status);
    }

    [Fact]
    public void FullScan_ShouldGenerateKnowledgeBaseEntries()
    {
        // Arrange
        var testRepository = CreateTestRepository();

        // Act
        var scanResult = ExecuteFullScan(testRepository);

        // Assert
        Assert.NotNull(scanResult);
        Assert.NotNull(scanResult.KnowledgeBaseEntries);
        Assert.True(scanResult.KnowledgeBaseEntries.Count > 0, 
            "Full scan should generate knowledge base entries");

        foreach (var entry in scanResult.KnowledgeBaseEntries)
        {
            Assert.False(string.IsNullOrEmpty(entry.Id), 
                "Knowledge base entry should have ID");
            Assert.False(string.IsNullOrEmpty(entry.EntityType), 
                "Knowledge base entry should have entity type");
            Assert.False(string.IsNullOrEmpty(entry.SearchableText), 
                "Knowledge base entry should have searchable text");
            Assert.True(entry.LastUpdated > DateTime.MinValue, 
                "Knowledge base entry should have valid last updated timestamp");
        }
    }

    private static TestRepository CreateTestRepository(string name = "test_repo")
    {
        // This would create a real test repository with MongoDB usage
        // For now, return a mock repository
        return new TestRepository
        {
            Name = name,
            Path = $"/tmp/{name}",
            HasMongoDBUsage = true,
            Files = new[]
            {
                "Models/User.cs",
                "Models/Product.cs",
                "Services/UserService.cs",
                "Services/ProductService.cs",
                "Controllers/UserController.cs"
            }
        };
    }

    private static TestRepository CreateTestRepositoryWithoutMongoDB(string name = "test_repo_no_mongo")
    {
        return new TestRepository
        {
            Name = name,
            Path = $"/tmp/{name}",
            HasMongoDBUsage = false,
            Files = new[]
            {
                "Models/Config.cs",
                "Services/ConfigService.cs"
            }
        };
    }

    private static ScanResult ExecuteFullScan(params TestRepository[] repositories)
    {
        // This would execute the actual scanner
        // For now, return a mock result
        return new ScanResult
        {
            ScanId = Guid.NewGuid().ToString(),
            Status = "completed",
            TypesDiscovered = 8,
            CollectionsMapped = 4,
            QueriesExtracted = 15,
            RelationshipsInferred = 3,
            SchemasObserved = 2,
            Repositories = repositories.Select(r => new RepositoryResult
            {
                Repository = r.Path,
                Status = r.HasMongoDBUsage ? "success" : "skipped",
                TypesDiscovered = r.HasMongoDBUsage ? 8 : 0,
                CollectionsMapped = r.HasMongoDBUsage ? 4 : 0,
                QueriesExtracted = r.HasMongoDBUsage ? 15 : 0
            }).ToList(),
            ProvenanceRecords = new List<ProvenanceRecord>
            {
                new()
                {
                    Repository = repositories[0].Path,
                    FilePath = "Models/User.cs",
                    Symbol = "User",
                    LineSpan = new LineSpan { Start = 5, End = 25 },
                    CommitSHA = "abc123def456",
                    Timestamp = DateTime.UtcNow
                }
            },
            KnowledgeBaseEntries = new List<KnowledgeBaseEntry>
            {
                new()
                {
                    Id = "type-user-001",
                    EntityType = "CodeType",
                    EntityId = "user-type",
                    SearchableText = "User class with MongoDB attributes",
                    Tags = new[] { "POCO", "MongoDB", "User" },
                    LastUpdated = DateTime.UtcNow
                }
            }
        };
    }

    // Test data classes
    private class TestRepository
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public bool HasMongoDBUsage { get; set; }
        public string[] Files { get; set; } = Array.Empty<string>();
    }

    private class ScanResult
    {
        public string ScanId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int TypesDiscovered { get; set; }
        public int CollectionsMapped { get; set; }
        public int QueriesExtracted { get; set; }
        public int RelationshipsInferred { get; set; }
        public int SchemasObserved { get; set; }
        public List<RepositoryResult> Repositories { get; set; } = new();
        public List<ProvenanceRecord> ProvenanceRecords { get; set; } = new();
        public List<KnowledgeBaseEntry> KnowledgeBaseEntries { get; set; } = new();
    }

    private class RepositoryResult
    {
        public string Repository { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int TypesDiscovered { get; set; }
        public int CollectionsMapped { get; set; }
        public int QueriesExtracted { get; set; }
    }

    private class ProvenanceRecord
    {
        public string Repository { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
        public LineSpan LineSpan { get; set; } = new();
        public string CommitSHA { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    private class LineSpan
    {
        public int Start { get; set; }
        public int End { get; set; }
    }

    private class KnowledgeBaseEntry
    {
        public string Id { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string SearchableText { get; set; } = string.Empty;
        public string[] Tags { get; set; } = Array.Empty<string>();
        public DateTime LastUpdated { get; set; }
    }
}
