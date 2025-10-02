namespace CatalogApi.Configuration;

/// <summary>
/// Configuration options for the Catalog API
/// </summary>
public class ApiConfiguration
{
    public const string SectionName = "ApiSettings";

    /// <summary>
    /// Maximum number of results to return in a single search request
    /// </summary>
    public int MaxSearchResults { get; set; } = 1000;

    /// <summary>
    /// Default number of results to return in a search request
    /// </summary>
    public int DefaultSearchResults { get; set; } = 50;

    /// <summary>
    /// Maximum depth for graph traversal
    /// </summary>
    public int MaxGraphDepth { get; set; } = 5;

    /// <summary>
    /// Default depth for graph traversal
    /// </summary>
    public int DefaultGraphDepth { get; set; } = 2;

    /// <summary>
    /// Maximum number of nodes to return in a graph response
    /// </summary>
    public int MaxGraphNodes { get; set; } = 500;

    /// <summary>
    /// Default number of nodes to return in a graph response
    /// </summary>
    public int DefaultGraphNodes { get; set; } = 100;

    /// <summary>
    /// Enable detailed error responses in development
    /// </summary>
    public bool EnableDetailedErrors { get; set; } = false;

    /// <summary>
    /// Enable request/response logging
    /// </summary>
    public bool EnableRequestLogging { get; set; } = true;

    /// <summary>
    /// Enable response caching
    /// </summary>
    public bool EnableCaching { get; set; } = true;

    /// <summary>
    /// API version
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// API title
    /// </summary>
    public string Title { get; set; } = "Catalog API";

    /// <summary>
    /// API description
    /// </summary>
    public string Description { get; set; } = "API for searching and exploring code intelligence knowledge base";

    /// <summary>
    /// Contact information
    /// </summary>
    public ContactInfo Contact { get; set; } = new();

    /// <summary>
    /// License information
    /// </summary>
    public LicenseInfo License { get; set; } = new();
}

/// <summary>
/// Contact information for the API
/// </summary>
public class ContactInfo
{
    /// <summary>
    /// Contact name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Contact email
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Contact URL
    /// </summary>
    public string Url { get; set; } = string.Empty;
}

/// <summary>
/// License information for the API
/// </summary>
public class LicenseInfo
{
    /// <summary>
    /// License name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// License URL
    /// </summary>
    public string Url { get; set; } = string.Empty;
}