using System.Diagnostics;

namespace CatalogApi.Services;

/// <summary>
/// Observability service implementation using .NET logging and diagnostics
/// </summary>
public class ObservabilityService : IObservabilityService
{
    private readonly ILogger<ObservabilityService> _logger;
    private readonly ActivitySource _activitySource;
    private static readonly ActivitySource DefaultActivitySource = new("CatalogApi");

    public ObservabilityService(ILogger<ObservabilityService> logger)
    {
        _logger = logger;
        _activitySource = DefaultActivitySource;
    }

    public void LogInformation(string message, Dictionary<string, object>? properties = null)
    {
        if (properties != null && properties.Any())
        {
            _logger.LogInformation("{Message} {Properties}", message, properties);
        }
        else
        {
            _logger.LogInformation("{Message}", message);
        }
    }

    public void LogWarning(string message, Exception? exception = null, Dictionary<string, object>? properties = null)
    {
        if (exception != null)
        {
            if (properties != null && properties.Any())
            {
                _logger.LogWarning(exception, "{Message} {Properties}", message, properties);
            }
            else
            {
                _logger.LogWarning(exception, "{Message}", message);
            }
        }
        else
        {
            if (properties != null && properties.Any())
            {
                _logger.LogWarning("{Message} {Properties}", message, properties);
            }
            else
            {
                _logger.LogWarning("{Message}", message);
            }
        }
    }

    public void LogError(string message, Exception? exception = null, Dictionary<string, object>? properties = null)
    {
        if (exception != null)
        {
            if (properties != null && properties.Any())
            {
                _logger.LogError(exception, "{Message} {Properties}", message, properties);
            }
            else
            {
                _logger.LogError(exception, "{Message}", message);
            }
        }
        else
        {
            if (properties != null && properties.Any())
            {
                _logger.LogError("{Message} {Properties}", message, properties);
            }
            else
            {
                _logger.LogError("{Message}", message);
            }
        }
    }

    public void LogDebug(string message, Dictionary<string, object>? properties = null)
    {
        if (properties != null && properties.Any())
        {
            _logger.LogDebug("{Message} {Properties}", message, properties);
        }
        else
        {
            _logger.LogDebug("{Message}", message);
        }
    }

    public IDisposable StartActivity(string name, Dictionary<string, object>? properties = null)
    {
        var activity = _activitySource.StartActivity(name);
        
        if (activity != null && properties != null)
        {
            foreach (var property in properties)
            {
                activity.SetTag(property.Key, property.Value?.ToString() ?? string.Empty);
            }
        }

        return new ActivityScope(() => activity?.Dispose());
    }

    public void RecordMetric(string name, double value, Dictionary<string, string>? tags = null)
    {
        // In a real implementation, this would integrate with a metrics system like Prometheus
        // For now, we'll just log the metric
        var tagsString = tags != null ? string.Join(", ", tags.Select(t => $"{t.Key}={t.Value}")) : "";
        _logger.LogDebug("Metric: {Name} = {Value} {Tags}", name, value, tagsString);
    }

    public void IncrementCounter(string name, double increment = 1, Dictionary<string, string>? tags = null)
    {
        // In a real implementation, this would integrate with a metrics system like Prometheus
        // For now, we'll just log the counter increment
        var tagsString = tags != null ? string.Join(", ", tags.Select(t => $"{t.Key}={t.Value}")) : "";
        _logger.LogDebug("Counter: {Name} += {Increment} {Tags}", name, increment, tagsString);
    }

    public void RecordHistogram(string name, double value, Dictionary<string, string>? tags = null)
    {
        // In a real implementation, this would integrate with a metrics system like Prometheus
        // For now, we'll just log the histogram value
        var tagsString = tags != null ? string.Join(", ", tags.Select(t => $"{t.Key}={t.Value}")) : "";
        _logger.LogDebug("Histogram: {Name} = {Value} {Tags}", name, value, tagsString);
    }

    public string GetTraceId()
    {
        return Activity.Current?.TraceId.ToString() ?? string.Empty;
    }

    public string GetSpanId()
    {
        return Activity.Current?.SpanId.ToString() ?? string.Empty;
    }
}
