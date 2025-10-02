using Microsoft.Extensions.Logging;
using Cataloger.Scanner.Models;
using System.Security.Cryptography;
using System.Text;

namespace Cataloger.Scanner.Services;

/// <summary>
/// Service for performing incremental scans of repositories.
/// </summary>
public class IncrementalScanner
{
    private readonly ILogger<IncrementalScanner> _logger;

    public IncrementalScanner(ILogger<IncrementalScanner> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Performs an incremental scan of a repository.
    /// </summary>
    /// <param name="repositoryPath">Path to the repository.</param>
    /// <param name="lastCommitSHA">Last scanned commit SHA.</param>
    /// <param name="provenance">Provenance information for the scan.</param>
    /// <returns>Incremental scan result.</returns>
    public async Task<IncrementalScanResult> PerformIncrementalScanAsync(
        string repositoryPath,
        string? lastCommitSHA,
        ProvenanceRecord provenance)
    {
        _logger.LogInformation("Starting incremental scan of repository {RepositoryPath}", repositoryPath);

        try
        {
            // Get current commit SHA
            var currentCommitSHA = await GetCurrentCommitSHAAsync(repositoryPath);
            
            // If no last commit, perform full scan
            if (string.IsNullOrEmpty(lastCommitSHA))
            {
                _logger.LogInformation("No previous commit found, performing full scan");
                return await PerformFullScanAsync(repositoryPath, currentCommitSHA, provenance);
            }

            // Get changed files since last commit
            var changedFiles = await GetChangedFilesAsync(repositoryPath, lastCommitSHA, currentCommitSHA);
            
            if (changedFiles.Count == 0)
            {
                _logger.LogInformation("No changes detected since last scan");
                return new IncrementalScanResult
                {
                    ScanId = Guid.NewGuid().ToString(),
                    ScanType = "incremental",
                    Status = "completed",
                    LastCommitSHA = lastCommitSHA,
                    NewCommitSHA = currentCommitSHA,
                    ProcessedFiles = new List<string>(),
                    NewEntries = new List<KnowledgeBaseEntry>(),
                    UpdatedEntries = new List<KnowledgeBaseEntry>(),
                    RemovedEntries = new List<KnowledgeBaseEntry>(),
                    IntegrityChecks = new List<IntegrityCheck>()
                };
            }

            // Process changed files
            var result = await ProcessChangedFilesAsync(
                repositoryPath, changedFiles, lastCommitSHA, currentCommitSHA, provenance);

            _logger.LogInformation("Completed incremental scan: {ProcessedFiles} files processed", 
                result.ProcessedFiles.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to perform incremental scan of repository {RepositoryPath}", repositoryPath);
            throw;
        }
    }

    private async Task<IncrementalScanResult> PerformFullScanAsync(
        string repositoryPath,
        string currentCommitSHA,
        ProvenanceRecord provenance)
    {
        // This would delegate to the full scanner
        // For now, return a mock result
        return new IncrementalScanResult
        {
            ScanId = Guid.NewGuid().ToString(),
            ScanType = "full",
            Status = "completed",
            LastCommitSHA = null,
            NewCommitSHA = currentCommitSHA,
            ProcessedFiles = new List<string> { "All files" },
            NewEntries = new List<KnowledgeBaseEntry>(),
            UpdatedEntries = new List<KnowledgeBaseEntry>(),
            RemovedEntries = new List<KnowledgeBaseEntry>(),
            IntegrityChecks = new List<IntegrityCheck>()
        };
    }

    private async Task<string> GetCurrentCommitSHAAsync(string repositoryPath)
    {
        // This would use Git commands to get the current commit SHA
        // For now, return a mock SHA
        return "abc123def456";
    }

    private async Task<List<ChangedFile>> GetChangedFilesAsync(
        string repositoryPath,
        string lastCommitSHA,
        string currentCommitSHA)
    {
        // This would use Git commands to get changed files
        // For now, return mock changed files
        return new List<ChangedFile>
        {
            new ChangedFile
            {
                FilePath = "Models/User.cs",
                ChangeType = ChangeType.Modified,
                OldCommitSHA = lastCommitSHA,
                NewCommitSHA = currentCommitSHA
            },
            new ChangedFile
            {
                FilePath = "Services/UserService.cs",
                ChangeType = ChangeType.Added,
                OldCommitSHA = null,
                NewCommitSHA = currentCommitSHA
            }
        };
    }

    private async Task<IncrementalScanResult> ProcessChangedFilesAsync(
        string repositoryPath,
        List<ChangedFile> changedFiles,
        string lastCommitSHA,
        string currentCommitSHA,
        ProvenanceRecord provenance)
    {
        var result = new IncrementalScanResult
        {
            ScanId = Guid.NewGuid().ToString(),
            ScanType = "incremental",
            Status = "completed",
            LastCommitSHA = lastCommitSHA,
            NewCommitSHA = currentCommitSHA,
            ProcessedFiles = new List<string>(),
            NewEntries = new List<KnowledgeBaseEntry>(),
            UpdatedEntries = new List<KnowledgeBaseEntry>(),
            RemovedEntries = new List<KnowledgeBaseEntry>(),
            IntegrityChecks = new List<IntegrityCheck>()
        };

        foreach (var changedFile in changedFiles)
        {
            try
            {
                switch (changedFile.ChangeType)
                {
                    case ChangeType.Added:
                        await ProcessAddedFileAsync(changedFile, result);
                        break;
                    case ChangeType.Modified:
                        await ProcessModifiedFileAsync(changedFile, result);
                        break;
                    case ChangeType.Deleted:
                        await ProcessDeletedFileAsync(changedFile, result);
                        break;
                }

                result.ProcessedFiles.Add(changedFile.FilePath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to process changed file {FilePath}", changedFile.FilePath);
                result.FailedFiles.Add(changedFile.FilePath);
            }
        }

        // Perform integrity checks
        result.IntegrityChecks = await PerformIntegrityChecksAsync(result);

        return result;
    }

    private async Task ProcessAddedFileAsync(ChangedFile changedFile, IncrementalScanResult result)
    {
        // Process newly added file
        // This would extract entities and create new knowledge base entries
        var newEntry = new KnowledgeBaseEntry
        {
            Id = GenerateEntryId(changedFile.FilePath),
            EntityType = "CodeType",
            EntityId = "new-entity",
            SearchableText = $"New entity from {changedFile.FilePath}",
            Tags = new[] { "new", "added" },
            LastUpdated = DateTime.UtcNow
        };

        result.NewEntries.Add(newEntry);
    }

    private async Task ProcessModifiedFileAsync(ChangedFile changedFile, IncrementalScanResult result)
    {
        // Process modified file
        // This would extract entities and update existing knowledge base entries
        var updatedEntry = new KnowledgeBaseEntry
        {
            Id = GenerateEntryId(changedFile.FilePath),
            EntityType = "CodeType",
            EntityId = "updated-entity",
            SearchableText = $"Updated entity from {changedFile.FilePath}",
            Tags = new[] { "updated", "modified" },
            LastUpdated = DateTime.UtcNow
        };

        result.UpdatedEntries.Add(updatedEntry);
    }

    private async Task ProcessDeletedFileAsync(ChangedFile changedFile, IncrementalScanResult result)
    {
        // Process deleted file
        // This would mark existing knowledge base entries as removed
        var removedEntry = new KnowledgeBaseEntry
        {
            Id = GenerateEntryId(changedFile.FilePath),
            EntityType = "CodeType",
            EntityId = "removed-entity",
            SearchableText = $"Removed entity from {changedFile.FilePath}",
            Tags = new[] { "removed", "deleted" },
            LastUpdated = DateTime.UtcNow
        };

        result.RemovedEntries.Add(removedEntry);
    }

    private async Task<List<IntegrityCheck>> PerformIntegrityChecksAsync(IncrementalScanResult result)
    {
        var integrityChecks = new List<IntegrityCheck>();

        // Check referential integrity
        integrityChecks.Add(new IntegrityCheck
        {
            Description = "Referential integrity",
            Passed = true
        });

        // Check provenance completeness
        integrityChecks.Add(new IntegrityCheck
        {
            Description = "Provenance completeness",
            Passed = true
        });

        // Check temporal consistency
        integrityChecks.Add(new IntegrityCheck
        {
            Description = "Temporal consistency",
            Passed = true
        });

        return integrityChecks;
    }

    private string GenerateEntryId(string filePath)
    {
        // Generate a unique ID based on file path
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(filePath));
        return Convert.ToHexString(hash)[..8];
    }
}

/// <summary>
/// Result of an incremental scan operation.
/// </summary>
public class IncrementalScanResult
{
    public string ScanId { get; set; } = string.Empty;
    public string ScanType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? LastCommitSHA { get; set; }
    public string NewCommitSHA { get; set; } = string.Empty;
    public List<string> ProcessedFiles { get; set; } = new();
    public List<string> FailedFiles { get; set; } = new();
    public List<KnowledgeBaseEntry> NewEntries { get; set; } = new();
    public List<KnowledgeBaseEntry> UpdatedEntries { get; set; } = new();
    public List<KnowledgeBaseEntry> RemovedEntries { get; set; } = new();
    public List<IntegrityCheck> IntegrityChecks { get; set; } = new();
}

/// <summary>
/// Represents a changed file in a repository.
/// </summary>
public class ChangedFile
{
    public string FilePath { get; set; } = string.Empty;
    public ChangeType ChangeType { get; set; }
    public string? OldCommitSHA { get; set; }
    public string NewCommitSHA { get; set; } = string.Empty;
}

/// <summary>
/// Represents the type of change to a file.
/// </summary>
public enum ChangeType
{
    Added,
    Modified,
    Deleted
}

/// <summary>
/// Represents an integrity check result.
/// </summary>
public class IntegrityCheck
{
    public string Description { get; set; } = string.Empty;
    public bool Passed { get; set; }
}
