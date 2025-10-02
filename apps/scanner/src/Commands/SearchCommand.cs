using System.CommandLine;
using Microsoft.Extensions.Logging;
using Cataloger.Scanner.Models;
using MongoDB.Driver;
using System.Text.Json;

namespace Cataloger.Scanner.Commands;

/// <summary>
/// CLI command for searching the knowledge base.
/// </summary>
public class SearchCommand
{
    private readonly ILogger<SearchCommand> _logger;
    private readonly IMongoDatabase _database;

    public SearchCommand(ILogger<SearchCommand> logger, IMongoDatabase database)
    {
        _logger = logger;
        _database = database;
    }

    /// <summary>
    /// Creates the search command with all options and arguments.
    /// </summary>
    /// <returns>The configured search command.</returns>
    public static Command CreateCommand()
    {
        var queryArgument = new Argument<string>(
            "query",
            "Search query text");

        var entityTypesOption = new Option<string[]>(
            "--entity-types",
            "Filter by entity types (CodeType, CollectionMapping, QueryOperation, DataRelationship, ObservedSchema)")
        {
            IsRequired = false
        };

        var repositoriesOption = new Option<string[]>(
            "--repositories",
            "Filter by repositories")
        {
            IsRequired = false
        };

        var limitOption = new Option<int>(
            "--limit",
            () => 50,
            "Maximum number of results to return")
        {
            IsRequired = false
        };

        var offsetOption = new Option<int>(
            "--offset",
            () => 0,
            "Number of results to skip")
        {
            IsRequired = false
        };

        var outputFormatOption = new Option<string>(
            "--output-format",
            () => "json",
            "Output format for search results (json, yaml, csv)")
        {
            IsRequired = false
        };

        var outputFileOption = new Option<string>(
            "--output-file",
            "Output file path for search results")
        {
            IsRequired = false
        };

        var verboseOption = new Option<bool>(
            "--verbose",
            () => false,
            "Enable verbose logging")
        {
            IsRequired = false
        };

        var command = new Command("search", "Search the knowledge base")
        {
            queryArgument,
            entityTypesOption,
            repositoriesOption,
            limitOption,
            offsetOption,
            outputFormatOption,
            outputFileOption,
            verboseOption
        };

        command.SetHandler(async (query, entityTypes, repositories, limit, offset, outputFormat, outputFile, verbose) =>
        {
            var searchCommand = new SearchCommand(
                null!, // Logger would be injected
                null!); // Database would be injected

            await searchCommand.ExecuteAsync(
                query,
                entityTypes,
                repositories,
                limit,
                offset,
                outputFormat,
                outputFile,
                verbose);
        }, queryArgument, entityTypesOption, repositoriesOption, limitOption, offsetOption, outputFormatOption, outputFileOption, verboseOption);

        return command;
    }

    /// <summary>
    /// Executes the search command.
    /// </summary>
    /// <param name="query">Search query text.</param>
    /// <param name="entityTypes">Entity types to filter by.</param>
    /// <param name="repositories">Repositories to filter by.</param>
    /// <param name="limit">Maximum number of results.</param>
    /// <param name="offset">Number of results to skip.</param>
    /// <param name="outputFormat">Output format.</param>
    /// <param name="outputFile">Output file path.</param>
    /// <param name="verbose">Whether verbose logging is enabled.</param>
    /// <returns>Task representing the async operation.</returns>
    public async Task ExecuteAsync(
        string query,
        string[]? entityTypes,
        string[]? repositories,
        int limit,
        int offset,
        string outputFormat,
        string? outputFile,
        bool verbose)
    {
        _logger?.LogInformation("Searching knowledge base for: {Query}", query);

        try
        {
            // Validate parameters
            if (limit < 1 || limit > 1000)
            {
                throw new ArgumentException("Limit must be between 1 and 1000");
            }

            if (offset < 0)
            {
                throw new ArgumentException("Offset must be non-negative");
            }

            // Perform search
            var searchResult = await PerformSearchAsync(query, entityTypes, repositories, limit, offset);

            // Output results
            await OutputResultsAsync(searchResult, outputFormat, outputFile);

            _logger?.LogInformation("Search completed: {ResultCount} results found", searchResult.Results.Count);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Search failed");
            throw;
        }
    }

    private async Task<SearchResult> PerformSearchAsync(
        string query,
        string[]? entityTypes,
        string[]? repositories,
        int limit,
        int offset)
    {
        var searchResult = new SearchResult
        {
            Query = query,
            Limit = limit,
            Offset = offset,
            Results = new List<SearchResultItem>()
        };

        try
        {
            // Search knowledge base entries
            var knowledgeBaseEntries = await SearchKnowledgeBaseEntriesAsync(query, entityTypes, repositories, limit, offset);
            
            // Convert to search result items
            foreach (var entry in knowledgeBaseEntries)
            {
                var resultItem = new SearchResultItem
                {
                    Id = entry.Id,
                    EntityType = entry.EntityType,
                    Title = GenerateTitle(entry),
                    Description = entry.SearchableText,
                    RelevanceScore = entry.RelevanceScore,
                    Repository = entry.Provenance.Repository,
                    FilePath = entry.Provenance.FilePath,
                    LineNumber = entry.Provenance.LineSpan.Start
                };

                searchResult.Results.Add(resultItem);
            }

            // Get total count
            searchResult.Total = await GetTotalCountAsync(query, entityTypes, repositories);

            // Sort by relevance score
            searchResult.Results = searchResult.Results
                .OrderByDescending(r => r.RelevanceScore)
                .ToList();

            return searchResult;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to perform search");
            throw;
        }
    }

    private async Task<List<KnowledgeBaseEntry>> SearchKnowledgeBaseEntriesAsync(
        string query,
        string[]? entityTypes,
        string[]? repositories,
        int limit,
        int offset)
    {
        var collection = _database.GetCollection<KnowledgeBaseEntry>("knowledge_base_entries");
        
        // Build filter
        var filterBuilder = Builders<KnowledgeBaseEntry>.Filter;
        var filters = new List<FilterDefinition<KnowledgeBaseEntry>>();

        // Text search filter
        filters.Add(filterBuilder.Text(query));

        // Entity type filter
        if (entityTypes != null && entityTypes.Length > 0)
        {
            filters.Add(filterBuilder.In(kbe => kbe.EntityType, entityTypes));
        }

        // Repository filter
        if (repositories != null && repositories.Length > 0)
        {
            filters.Add(filterBuilder.In(kbe => kbe.Provenance.Repository, repositories));
        }

        // Active entries only
        filters.Add(filterBuilder.Eq(kbe => kbe.IsActive, true));

        var filter = filterBuilder.And(filters);

        // Execute query
        var cursor = await collection.Find(filter)
            .Skip(offset)
            .Limit(limit)
            .ToCursorAsync();

        var results = new List<KnowledgeBaseEntry>();
        while (await cursor.MoveNextAsync())
        {
            results.AddRange(cursor.Current);
        }

        return results;
    }

    private async Task<int> GetTotalCountAsync(
        string query,
        string[]? entityTypes,
        string[]? repositories)
    {
        var collection = _database.GetCollection<KnowledgeBaseEntry>("knowledge_base_entries");
        
        // Build filter (same as search)
        var filterBuilder = Builders<KnowledgeBaseEntry>.Filter;
        var filters = new List<FilterDefinition<KnowledgeBaseEntry>>();

        filters.Add(filterBuilder.Text(query));

        if (entityTypes != null && entityTypes.Length > 0)
        {
            filters.Add(filterBuilder.In(kbe => kbe.EntityType, entityTypes));
        }

        if (repositories != null && repositories.Length > 0)
        {
            filters.Add(filterBuilder.In(kbe => kbe.Provenance.Repository, repositories));
        }

        filters.Add(filterBuilder.Eq(kbe => kbe.IsActive, true));

        var filter = filterBuilder.And(filters);

        // Get count
        return (int)await collection.CountDocumentsAsync(filter);
    }

    private string GenerateTitle(KnowledgeBaseEntry entry)
    {
        return entry.EntityType switch
        {
            "CodeType" => $"Type: {entry.EntityId}",
            "CollectionMapping" => $"Collection: {entry.EntityId}",
            "QueryOperation" => $"Operation: {entry.EntityId}",
            "DataRelationship" => $"Relationship: {entry.EntityId}",
            "ObservedSchema" => $"Schema: {entry.EntityId}",
            _ => entry.EntityId
        };
    }

    private async Task OutputResultsAsync(SearchResult searchResult, string outputFormat, string? outputFile)
    {
        string output;

        switch (outputFormat.ToLowerInvariant())
        {
            case "json":
                output = JsonSerializer.Serialize(searchResult, new JsonSerializerOptions { WriteIndented = true });
                break;
            case "yaml":
                // This would use a YAML serializer
                output = "YAML output not implemented";
                break;
            case "csv":
                // This would generate CSV output
                output = "CSV output not implemented";
                break;
            default:
                throw new ArgumentException($"Unsupported output format: {outputFormat}");
        }

        if (!string.IsNullOrEmpty(outputFile))
        {
            await File.WriteAllTextAsync(outputFile, output);
            _logger?.LogInformation("Search results written to {OutputFile}", outputFile);
        }
        else
        {
            Console.WriteLine(output);
        }
    }
}

/// <summary>
/// Result of a search operation.
/// </summary>
public class SearchResult
{
    public string Query { get; set; } = string.Empty;
    public List<SearchResultItem> Results { get; set; } = new();
    public int Total { get; set; }
    public int Limit { get; set; }
    public int Offset { get; set; }
}

/// <summary>
/// Individual search result item.
/// </summary>
public class SearchResultItem
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
