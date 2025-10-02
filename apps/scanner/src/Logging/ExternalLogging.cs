using Microsoft.Extensions.Logging;
using System.Text.Json;
using Cataloger.Scanner.Configuration;

namespace Cataloger.Scanner.Logging;

/// <summary>
/// External logging integration service for the Code Intelligence Scanner.
/// </summary>
public class ExternalLoggingService
{
    private readonly ILogger<ExternalLoggingService> _logger;
    private readonly ExternalLoggingConfiguration _configuration;
    private readonly Dictionary<string, IExternalLoggingProvider> _providers;

    public ExternalLoggingService(
        ILogger<ExternalLoggingService> logger,
        ExternalLoggingConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _providers = new Dictionary<string, IExternalLoggingProvider>();
        
        InitializeProviders();
    }

    /// <summary>
    /// Logs a message to external logging systems.
    /// </summary>
    /// <param name="level">Log level.</param>
    /// <param name="message">Log message.</param>
    /// <param name="properties">Additional properties.</param>
    /// <param name="exception">Exception if any.</param>
    public async Task LogAsync(
        LogLevel level,
        string message,
        Dictionary<string, object>? properties = null,
        Exception? exception = null)
    {
        try
        {
            var logEntry = new ExternalLogEntry
            {
                Timestamp = DateTime.UtcNow,
                Level = level.ToString(),
                Message = message,
                Properties = properties ?? new Dictionary<string, object>(),
                Exception = exception?.ToString(),
                Source = "Cataloger.Scanner",
                Version = GetAssemblyVersion()
            };

            // Add common properties
            logEntry.Properties["MachineName"] = Environment.MachineName;
            logEntry.Properties["ProcessId"] = Environment.ProcessId;
            logEntry.Properties["ThreadId"] = Environment.CurrentManagedThreadId;

            // Send to all configured providers
            var tasks = _providers.Values.Select(provider => 
                provider.LogAsync(logEntry).ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        _logger.LogError(t.Exception, "Failed to send log to external provider");
                    }
                }));

            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log to external systems");
        }
    }

    /// <summary>
    /// Logs a structured event to external logging systems.
    /// </summary>
    /// <param name="eventName">Event name.</param>
    /// <param name="properties">Event properties.</param>
    /// <param name="metrics">Event metrics.</param>
    public async Task LogEventAsync(
        string eventName,
        Dictionary<string, object>? properties = null,
        Dictionary<string, double>? metrics = null)
    {
        try
        {
            var eventEntry = new ExternalEventEntry
            {
                Timestamp = DateTime.UtcNow,
                EventName = eventName,
                Properties = properties ?? new Dictionary<string, object>(),
                Metrics = metrics ?? new Dictionary<string, double>(),
                Source = "Cataloger.Scanner",
                Version = GetAssemblyVersion()
            };

            // Add common properties
            eventEntry.Properties["MachineName"] = Environment.MachineName;
            eventEntry.Properties["ProcessId"] = Environment.ProcessId;

            // Send to all configured providers
            var tasks = _providers.Values.Select(provider => 
                provider.LogEventAsync(eventEntry).ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        _logger.LogError(t.Exception, "Failed to send event to external provider");
                    }
                }));

            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log event to external systems");
        }
    }

    /// <summary>
    /// Logs performance metrics to external logging systems.
    /// </summary>
    /// <param name="operation">Operation name.</param>
    /// <param name="duration">Operation duration.</param>
    /// <param name="metrics">Additional metrics.</param>
    public async Task LogMetricsAsync(
        string operation,
        TimeSpan duration,
        Dictionary<string, double>? metrics = null)
    {
        try
        {
            var metricsEntry = new ExternalMetricsEntry
            {
                Timestamp = DateTime.UtcNow,
                Operation = operation,
                Duration = duration.TotalMilliseconds,
                Metrics = metrics ?? new Dictionary<string, double>(),
                Source = "Cataloger.Scanner",
                Version = GetAssemblyVersion()
            };

            // Add common metrics
            metricsEntry.Metrics["MemoryUsageMB"] = GC.GetTotalMemory(false) / (1024.0 * 1024.0);
            metricsEntry.Metrics["ThreadCount"] = Environment.ProcessorCount;

            // Send to all configured providers
            var tasks = _providers.Values.Select(provider => 
                provider.LogMetricsAsync(metricsEntry).ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        _logger.LogError(t.Exception, "Failed to send metrics to external provider");
                    }
                }));

            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log metrics to external systems");
        }
    }

    /// <summary>
    /// Logs an error to external logging systems.
    /// </summary>
    /// <param name="error">Error information.</param>
    /// <param name="context">Additional context.</param>
    public async Task LogErrorAsync(
        ErrorInfo error,
        Dictionary<string, object>? context = null)
    {
        try
        {
            var errorEntry = new ExternalErrorEntry
            {
                Timestamp = DateTime.UtcNow,
                ErrorType = error.ErrorType,
                Message = error.Message,
                StackTrace = error.StackTrace,
                Context = context ?? new Dictionary<string, object>(),
                Source = "Cataloger.Scanner",
                Version = GetAssemblyVersion()
            };

            // Add common context
            errorEntry.Context["MachineName"] = Environment.MachineName;
            errorEntry.Context["ProcessId"] = Environment.ProcessId;
            errorEntry.Context["ThreadId"] = Environment.CurrentManagedThreadId;

            // Send to all configured providers
            var tasks = _providers.Values.Select(provider => 
                provider.LogErrorAsync(errorEntry).ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        _logger.LogError(t.Exception, "Failed to send error to external provider");
                    }
                }));

            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log error to external systems");
        }
    }

    /// <summary>
    /// Flushes all pending logs to external systems.
    /// </summary>
    public async Task FlushAsync()
    {
        try
        {
            var tasks = _providers.Values.Select(provider => 
                provider.FlushAsync().ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        _logger.LogError(t.Exception, "Failed to flush external provider");
                    }
                }));

            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to flush external logging systems");
        }
    }

    /// <summary>
    /// Gets the status of external logging providers.
    /// </summary>
    /// <returns>Provider status information.</returns>
    public async Task<Dictionary<string, ProviderStatus>> GetProviderStatusAsync()
    {
        var status = new Dictionary<string, ProviderStatus>();

        foreach (var provider in _providers)
        {
            try
            {
                var providerStatus = await provider.Value.GetStatusAsync();
                status[provider.Key] = providerStatus;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get status for provider {ProviderName}", provider.Key);
                status[provider.Key] = new ProviderStatus
                {
                    IsHealthy = false,
                    LastError = ex.Message,
                    LastCheck = DateTime.UtcNow
                };
            }
        }

        return status;
    }

    private void InitializeProviders()
    {
        if (!_configuration.Enabled)
        {
            _logger.LogInformation("External logging is disabled");
            return;
        }

        try
        {
            switch (_configuration.Provider.ToLowerInvariant())
            {
                case "applicationinsights":
                    _providers["ApplicationInsights"] = new ApplicationInsightsProvider(_configuration);
                    break;
                case "datadog":
                    _providers["DataDog"] = new DataDogProvider(_configuration);
                    break;
                case "newrelic":
                    _providers["NewRelic"] = new NewRelicProvider(_configuration);
                    break;
                case "elasticsearch":
                    _providers["Elasticsearch"] = new ElasticsearchProvider(_configuration);
                    break;
                case "splunk":
                    _providers["Splunk"] = new SplunkProvider(_configuration);
                    break;
                default:
                    _logger.LogWarning("Unknown external logging provider: {Provider}", _configuration.Provider);
                    break;
            }

            _logger.LogInformation("Initialized {ProviderCount} external logging providers", _providers.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize external logging providers");
        }
    }

    private string GetAssemblyVersion()
    {
        try
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            return version?.ToString() ?? "1.0.0";
        }
        catch
        {
            return "1.0.0";
        }
    }
}

/// <summary>
/// External logging provider interface.
/// </summary>
public interface IExternalLoggingProvider
{
    Task LogAsync(ExternalLogEntry logEntry);
    Task LogEventAsync(ExternalEventEntry eventEntry);
    Task LogMetricsAsync(ExternalMetricsEntry metricsEntry);
    Task LogErrorAsync(ExternalErrorEntry errorEntry);
    Task FlushAsync();
    Task<ProviderStatus> GetStatusAsync();
}

/// <summary>
/// External log entry.
/// </summary>
public class ExternalLogEntry
{
    public DateTime Timestamp { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, object> Properties { get; set; } = new();
    public string? Exception { get; set; }
    public string Source { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
}

/// <summary>
/// External event entry.
/// </summary>
public class ExternalEventEntry
{
    public DateTime Timestamp { get; set; }
    public string EventName { get; set; } = string.Empty;
    public Dictionary<string, object> Properties { get; set; } = new();
    public Dictionary<string, double> Metrics { get; set; } = new();
    public string Source { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
}

/// <summary>
/// External metrics entry.
/// </summary>
public class ExternalMetricsEntry
{
    public DateTime Timestamp { get; set; }
    public string Operation { get; set; } = string.Empty;
    public double Duration { get; set; }
    public Dictionary<string, double> Metrics { get; set; } = new();
    public string Source { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
}

/// <summary>
/// External error entry.
/// </summary>
public class ExternalErrorEntry
{
    public DateTime Timestamp { get; set; }
    public string ErrorType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public Dictionary<string, object> Context { get; set; } = new();
    public string Source { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
}

/// <summary>
/// Error information.
/// </summary>
public class ErrorInfo
{
    public string ErrorType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
}

/// <summary>
/// Provider status.
/// </summary>
public class ProviderStatus
{
    public bool IsHealthy { get; set; }
    public string? LastError { get; set; }
    public DateTime LastCheck { get; set; }
    public Dictionary<string, object> Details { get; set; } = new();
}

/// <summary>
/// Application Insights provider.
/// </summary>
public class ApplicationInsightsProvider : IExternalLoggingProvider
{
    private readonly ExternalLoggingConfiguration _configuration;

    public ApplicationInsightsProvider(ExternalLoggingConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task LogAsync(ExternalLogEntry logEntry)
    {
        // Implementation for Application Insights
        await Task.Delay(1); // Simulate API call
    }

    public async Task LogEventAsync(ExternalEventEntry eventEntry)
    {
        // Implementation for Application Insights events
        await Task.Delay(1); // Simulate API call
    }

    public async Task LogMetricsAsync(ExternalMetricsEntry metricsEntry)
    {
        // Implementation for Application Insights metrics
        await Task.Delay(1); // Simulate API call
    }

    public async Task LogErrorAsync(ExternalErrorEntry errorEntry)
    {
        // Implementation for Application Insights errors
        await Task.Delay(1); // Simulate API call
    }

    public async Task FlushAsync()
    {
        // Implementation for Application Insights flush
        await Task.Delay(1); // Simulate API call
    }

    public async Task<ProviderStatus> GetStatusAsync()
    {
        // Implementation for Application Insights status check
        await Task.Delay(1); // Simulate API call
        return new ProviderStatus
        {
            IsHealthy = true,
            LastCheck = DateTime.UtcNow
        };
    }
}

/// <summary>
/// DataDog provider.
/// </summary>
public class DataDogProvider : IExternalLoggingProvider
{
    private readonly ExternalLoggingConfiguration _configuration;

    public DataDogProvider(ExternalLoggingConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task LogAsync(ExternalLogEntry logEntry)
    {
        // Implementation for DataDog
        await Task.Delay(1); // Simulate API call
    }

    public async Task LogEventAsync(ExternalEventEntry eventEntry)
    {
        // Implementation for DataDog events
        await Task.Delay(1); // Simulate API call
    }

    public async Task LogMetricsAsync(ExternalMetricsEntry metricsEntry)
    {
        // Implementation for DataDog metrics
        await Task.Delay(1); // Simulate API call
    }

    public async Task LogErrorAsync(ExternalErrorEntry errorEntry)
    {
        // Implementation for DataDog errors
        await Task.Delay(1); // Simulate API call
    }

    public async Task FlushAsync()
    {
        // Implementation for DataDog flush
        await Task.Delay(1); // Simulate API call
    }

    public async Task<ProviderStatus> GetStatusAsync()
    {
        // Implementation for DataDog status check
        await Task.Delay(1); // Simulate API call
        return new ProviderStatus
        {
            IsHealthy = true,
            LastCheck = DateTime.UtcNow
        };
    }
}

/// <summary>
/// New Relic provider.
/// </summary>
public class NewRelicProvider : IExternalLoggingProvider
{
    private readonly ExternalLoggingConfiguration _configuration;

    public NewRelicProvider(ExternalLoggingConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task LogAsync(ExternalLogEntry logEntry)
    {
        // Implementation for New Relic
        await Task.Delay(1); // Simulate API call
    }

    public async Task LogEventAsync(ExternalEventEntry eventEntry)
    {
        // Implementation for New Relic events
        await Task.Delay(1); // Simulate API call
    }

    public async Task LogMetricsAsync(ExternalMetricsEntry metricsEntry)
    {
        // Implementation for New Relic metrics
        await Task.Delay(1); // Simulate API call
    }

    public async Task LogErrorAsync(ExternalErrorEntry errorEntry)
    {
        // Implementation for New Relic errors
        await Task.Delay(1); // Simulate API call
    }

    public async Task FlushAsync()
    {
        // Implementation for New Relic flush
        await Task.Delay(1); // Simulate API call
    }

    public async Task<ProviderStatus> GetStatusAsync()
    {
        // Implementation for New Relic status check
        await Task.Delay(1); // Simulate API call
        return new ProviderStatus
        {
            IsHealthy = true,
            LastCheck = DateTime.UtcNow
        };
    }
}

/// <summary>
/// Elasticsearch provider.
/// </summary>
public class ElasticsearchProvider : IExternalLoggingProvider
{
    private readonly ExternalLoggingConfiguration _configuration;

    public ElasticsearchProvider(ExternalLoggingConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task LogAsync(ExternalLogEntry logEntry)
    {
        // Implementation for Elasticsearch
        await Task.Delay(1); // Simulate API call
    }

    public async Task LogEventAsync(ExternalEventEntry eventEntry)
    {
        // Implementation for Elasticsearch events
        await Task.Delay(1); // Simulate API call
    }

    public async Task LogMetricsAsync(ExternalMetricsEntry metricsEntry)
    {
        // Implementation for Elasticsearch metrics
        await Task.Delay(1); // Simulate API call
    }

    public async Task LogErrorAsync(ExternalErrorEntry errorEntry)
    {
        // Implementation for Elasticsearch errors
        await Task.Delay(1); // Simulate API call
    }

    public async Task FlushAsync()
    {
        // Implementation for Elasticsearch flush
        await Task.Delay(1); // Simulate API call
    }

    public async Task<ProviderStatus> GetStatusAsync()
    {
        // Implementation for Elasticsearch status check
        await Task.Delay(1); // Simulate API call
        return new ProviderStatus
        {
            IsHealthy = true,
            LastCheck = DateTime.UtcNow
        };
    }
}

/// <summary>
/// Splunk provider.
/// </summary>
public class SplunkProvider : IExternalLoggingProvider
{
    private readonly ExternalLoggingConfiguration _configuration;

    public SplunkProvider(ExternalLoggingConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task LogAsync(ExternalLogEntry logEntry)
    {
        // Implementation for Splunk
        await Task.Delay(1); // Simulate API call
    }

    public async Task LogEventAsync(ExternalEventEntry eventEntry)
    {
        // Implementation for Splunk events
        await Task.Delay(1); // Simulate API call
    }

    public async Task LogMetricsAsync(ExternalMetricsEntry metricsEntry)
    {
        // Implementation for Splunk metrics
        await Task.Delay(1); // Simulate API call
    }

    public async Task LogErrorAsync(ExternalErrorEntry errorEntry)
    {
        // Implementation for Splunk errors
        await Task.Delay(1); // Simulate API call
    }

    public async Task FlushAsync()
    {
        // Implementation for Splunk flush
        await Task.Delay(1); // Simulate API call
    }

    public async Task<ProviderStatus> GetStatusAsync()
    {
        // Implementation for Splunk status check
        await Task.Delay(1); // Simulate API call
        return new ProviderStatus
        {
            IsHealthy = true,
            LastCheck = DateTime.UtcNow
        };
    }
}
