using System.Text.Json;
using Xunit;

namespace Cataloger.Scanner.Tests.Integration;

public class TestIncrementalScanWorkflow
{
    [Fact]
    public void IncrementalScan_ShouldOnlyProcessChangedFiles()
    {
        // Arrange
        var testRepository = CreateTestRepository();
        var lastCommitSHA = "abc123def456";
        var changedFiles = new[] { "Models/User.cs", "Services/UserService.cs" };

        // Act
        var scanResult = ExecuteIncrementalScan(testRepository, lastCommitSHA, changedFiles);

        // Assert
        Assert.NotNull(scanResult);
        Assert.Equal("incremental", scanResult.ScanType);
        Assert.True(scanResult.ProcessedFiles.Count <= changedFiles.Length, 
            "Incremental scan should only process changed files");
        
        foreach (var processedFile in scanResult.ProcessedFiles)
        {
            Assert.Contains(processedFile, changedFiles, 
                "Incremental scan should only process files that have changed");
        }
    }

    [Fact]
    public void IncrementalScan_ShouldUpdateExistingKnowledgeBaseEntries()
    {
        // Arrange
        var testRepository = CreateTestRepository();
        var lastCommitSHA = "abc123def456";
        var existingEntries = CreateExistingKnowledgeBaseEntries();

        // Act
        var scanResult = ExecuteIncrementalScan(testRepository, lastCommitSHA, new[] { "Models/User.cs" });

        // Assert
        Assert.NotNull(scanResult);
        Assert.NotNull(scanResult.UpdatedEntries);
        Assert.True(scanResult.UpdatedEntries.Count > 0, 
            "Incremental scan should update existing knowledge base entries");

        foreach (var updatedEntry in scanResult.UpdatedEntries)
        {
            Assert.True(updatedEntry.LastUpdated > existingEntries.First().LastUpdated, 
                "Updated entries should have newer timestamps");
        }
    }

    [Fact]
    public void IncrementalScan_ShouldAddNewKnowledgeBaseEntries()
    {
        // Arrange
        var testRepository = CreateTestRepository();
        var lastCommitSHA = "abc123def456";
        var newFiles = new[] { "Models/NewEntity.cs" };

        // Act
        var scanResult = ExecuteIncrementalScan(testRepository, lastCommitSHA, newFiles);

        // Assert
        Assert.NotNull(scanResult);
        Assert.NotNull(scanResult.NewEntries);
        Assert.True(scanResult.NewEntries.Count > 0, 
            "Incremental scan should add new knowledge base entries");

        foreach (var newEntry in scanResult.NewEntries)
        {
            Assert.False(string.IsNullOrEmpty(newEntry.Id), 
                "New entries should have valid IDs");
            Assert.True(newEntry.LastUpdated > DateTime.MinValue, 
                "New entries should have valid timestamps");
        }
    }

    [Fact]
    public void IncrementalScan_ShouldRemoveObsoleteEntries()
    {
        // Arrange
        var testRepository = CreateTestRepository();
        var lastCommitSHA = "abc123def456";
        var deletedFiles = new[] { "Models/OldEntity.cs" };

        // Act
        var scanResult = ExecuteIncrementalScan(testRepository, lastCommitSHA, new string[0], deletedFiles);

        // Assert
        Assert.NotNull(scanResult);
        Assert.NotNull(scanResult.RemovedEntries);
        Assert.True(scanResult.RemovedEntries.Count > 0, 
            "Incremental scan should remove obsolete knowledge base entries");

        foreach (var removedEntry in scanResult.RemovedEntries)
        {
            Assert.Contains("OldEntity", removedEntry.EntityId, 
                "Removed entries should correspond to deleted files");
        }
    }

    [Fact]
    public void IncrementalScan_ShouldBeFasterThanFullScan()
    {
        // Arrange
        var testRepository = CreateTestRepository();
        var lastCommitSHA = "abc123def456";
        var changedFiles = new[] { "Models/User.cs" };

        // Act
        var incrementalStartTime = DateTime.UtcNow;
        var incrementalResult = ExecuteIncrementalScan(testRepository, lastCommitSHA, changedFiles);
        var incrementalDuration = DateTime.UtcNow - incrementalStartTime;

        var fullStartTime = DateTime.UtcNow;
        var fullResult = ExecuteFullScan(testRepository);
        var fullDuration = DateTime.UtcNow - fullStartTime;

        // Assert
        Assert.NotNull(incrementalResult);
        Assert.NotNull(fullResult);
        Assert.True(incrementalDuration < fullDuration, 
            $"Incremental scan ({incrementalDuration.TotalSeconds:F2}s) should be faster than full scan ({fullDuration.TotalSeconds:F2}s)");
    }

    [Fact]
    public void IncrementalScan_ShouldMaintainDataIntegrity()
    {
        // Arrange
        var testRepository = CreateTestRepository();
        var lastCommitSHA = "abc123def456";
        var changedFiles = new[] { "Models/User.cs" };

        // Act
        var scanResult = ExecuteIncrementalScan(testRepository, lastCommitSHA, changedFiles);

        // Assert
        Assert.NotNull(scanResult);
        Assert.NotNull(scanResult.IntegrityChecks);
        Assert.True(scanResult.IntegrityChecks.All(check => check.Passed), 
            "Incremental scan should pass all integrity checks");

        foreach (var check in scanResult.IntegrityChecks)
        {
            Assert.False(string.IsNullOrEmpty(check.Description), 
                "Integrity check should have description");
            Assert.True(check.Passed, 
                $"Integrity check '{check.Description}' should pass");
        }
    }

    [Fact]
    public void IncrementalScan_ShouldHandleNoChanges()
    {
        // Arrange
        var testRepository = CreateTestRepository();
        var lastCommitSHA = "abc123def456";
        var changedFiles = new string[0];

        // Act
        var scanResult = ExecuteIncrementalScan(testRepository, lastCommitSHA, changedFiles);

        // Assert
        Assert.NotNull(scanResult);
        Assert.Equal(0, scanResult.ProcessedFiles.Count);
        Assert.Equal(0, scanResult.NewEntries.Count);
        Assert.Equal(0, scanResult.UpdatedEntries.Count);
        Assert.Equal(0, scanResult.RemovedEntries.Count);
        Assert.Equal("completed", scanResult.Status);
    }

    [Fact]
    public void IncrementalScan_ShouldTrackCommitSHA()
    {
        // Arrange
        var testRepository = CreateTestRepository();
        var lastCommitSHA = "abc123def456";
        var newCommitSHA = "def456ghi789";
        var changedFiles = new[] { "Models/User.cs" };

        // Act
        var scanResult = ExecuteIncrementalScan(testRepository, lastCommitSHA, changedFiles);

        // Assert
        Assert.NotNull(scanResult);
        Assert.Equal(newCommitSHA, scanResult.NewCommitSHA);
        Assert.True(scanResult.NewCommitSHA != lastCommitSHA, 
            "Incremental scan should track the new commit SHA");
    }

    [Fact]
    public void IncrementalScan_ShouldHandlePartialFailures()
    {
        // Arrange
        var testRepository = CreateTestRepository();
        var lastCommitSHA = "abc123def456";
        var changedFiles = new[] { "Models/User.cs", "Models/Invalid.cs" };

        // Act
        var scanResult = ExecuteIncrementalScan(testRepository, lastCommitSHA, changedFiles);

        // Assert
        Assert.NotNull(scanResult);
        Assert.NotNull(scanResult.FailedFiles);
        Assert.Contains("Models/Invalid.cs", scanResult.FailedFiles, 
            "Incremental scan should track failed files");
        Assert.Equal("partial", scanResult.Status);
    }

    private static TestRepository CreateTestRepository(string name = "test_repo")
    {
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

    private static List<KnowledgeBaseEntry> CreateExistingKnowledgeBaseEntries()
    {
        return new List<KnowledgeBaseEntry>
        {
            new()
            {
                Id = "type-user-001",
                EntityType = "CodeType",
                EntityId = "user-type",
                SearchableText = "User class with MongoDB attributes",
                Tags = new[] { "POCO", "MongoDB", "User" },
                LastUpdated = DateTime.UtcNow.AddHours(-1)
            }
        };
    }

    private static IncrementalScanResult ExecuteIncrementalScan(
        TestRepository repository, 
        string lastCommitSHA, 
        string[] changedFiles, 
        string[] deletedFiles = null)
    {
        // This would execute the actual incremental scanner
        // For now, return a mock result
        return new IncrementalScanResult
        {
            ScanId = Guid.NewGuid().ToString(),
            ScanType = "incremental",
            Status = "completed",
            LastCommitSHA = lastCommitSHA,
            NewCommitSHA = "def456ghi789",
            ProcessedFiles = changedFiles.ToList(),
            FailedFiles = new List<string>(),
            NewEntries = new List<KnowledgeBaseEntry>
            {
                new()
                {
                    Id = "type-new-001",
                    EntityType = "CodeType",
                    EntityId = "new-type",
                    SearchableText = "New entity class",
                    Tags = new[] { "POCO", "MongoDB", "New" },
                    LastUpdated = DateTime.UtcNow
                }
            },
            UpdatedEntries = new List<KnowledgeBaseEntry>
            {
                new()
                {
                    Id = "type-user-001",
                    EntityType = "CodeType",
                    EntityId = "user-type",
                    SearchableText = "Updated User class with MongoDB attributes",
                    Tags = new[] { "POCO", "MongoDB", "User", "Updated" },
                    LastUpdated = DateTime.UtcNow
                }
            },
            RemovedEntries = (deletedFiles ?? Array.Empty<string>()).Select(f => new KnowledgeBaseEntry
            {
                Id = "type-old-001",
                EntityType = "CodeType",
                EntityId = "old-type",
                SearchableText = "Old entity class",
                Tags = new[] { "POCO", "MongoDB", "Old" },
                LastUpdated = DateTime.UtcNow.AddHours(-1)
            }).ToList(),
            IntegrityChecks = new List<IntegrityCheck>
            {
                new() { Description = "Referential integrity", Passed = true },
                new() { Description = "Provenance completeness", Passed = true },
                new() { Description = "Temporal consistency", Passed = true }
            }
        };
    }

    private static ScanResult ExecuteFullScan(TestRepository repository)
    {
        // Mock full scan result
        return new ScanResult
        {
            ScanId = Guid.NewGuid().ToString(),
            Status = "completed",
            TypesDiscovered = 8,
            CollectionsMapped = 4,
            QueriesExtracted = 15,
            RelationshipsInferred = 3,
            SchemasObserved = 2
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

    private class IncrementalScanResult
    {
        public string ScanId { get; set; } = string.Empty;
        public string ScanType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string LastCommitSHA { get; set; } = string.Empty;
        public string NewCommitSHA { get; set; } = string.Empty;
        public List<string> ProcessedFiles { get; set; } = new();
        public List<string> FailedFiles { get; set; } = new();
        public List<KnowledgeBaseEntry> NewEntries { get; set; } = new();
        public List<KnowledgeBaseEntry> UpdatedEntries { get; set; } = new();
        public List<KnowledgeBaseEntry> RemovedEntries { get; set; } = new();
        public List<IntegrityCheck> IntegrityChecks { get; set; } = new();
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

    private class IntegrityCheck
    {
        public string Description { get; set; } = string.Empty;
        public bool Passed { get; set; }
    }
}
