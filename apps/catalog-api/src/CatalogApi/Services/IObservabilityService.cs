namespace CatalogApi.Services;

/// <summary>
/// Service interface for observability operations
/// </summary>
public interface IObservabilityService
{
    /// <summary>
    /// Log an information message
    /// </summary>
    /// <param name="message">Log message</param>
    /// <param name="properties">Additional properties</param>
    void LogInformation(string message, Dictionary<string, object>? properties = null);

    /// <summary>
    /// Log a warning message
    /// </summary>
    /// <param name="message">Log message</param>
    /// <param name="exception">Optional exception</param>
    /// <param name="properties">Additional properties</param>
    void LogWarning(string message, Exception? exception = null, Dictionary<string, object>? properties = null);

    /// <summary>
    /// Log an error message
    /// </summary>
    /// <param name="message">Log message</param>
    /// <param name="exception">Optional exception</param>
    /// <param name="properties">Additional properties</param>
    void LogError(string message, Exception? exception = null, Dictionary<string, object>? properties = null);

    /// <summary>
    /// Log a debug message
    /// </summary>
    /// <param name="message">Log message</param>
    /// <param name="properties">Additional properties</param>
    void LogDebug(string message, Dictionary<string, object>? properties = null);

    /// <summary>
    /// Start a new activity for tracing
    /// </summary>
    /// <param name="name">Activity name</param>
    /// <param name="properties">Activity properties</param>
    /// <returns>Activity scope</returns>
    IDisposable StartActivity(string name, Dictionary<string, object>? properties = null);

    /// <summary>
    /// Record a metric
    /// </summary>
    /// <param name="name">Metric name</param>
    /// <param name="value">Metric value</param>
    /// <param name="tags">Metric tags</param>
    void RecordMetric(string name, double value, Dictionary<string, string>? tags = null);

    /// <summary>
    /// Increment a counter metric
    /// </summary>
    /// <param name="name">Metric name</param>
    /// <param name="increment">Increment value</param>
    /// <param name="tags">Metric tags</param>
    void IncrementCounter(string name, double increment = 1, Dictionary<string, string>? tags = null);

    /// <summary>
    /// Record a histogram metric
    /// </summary>
    /// <param name="name">Metric name</param>
    /// <param name="value">Metric value</param>
    /// <param name="tags">Metric tags</param>
    void RecordHistogram(string name, double value, Dictionary<string, string>? tags = null);

    /// <summary>
    /// Get current trace ID
    /// </summary>
    /// <returns>Trace ID</returns>
    string GetTraceId();

    /// <summary>
    /// Get current span ID
    /// </summary>
    /// <returns>Span ID</returns>
    string GetSpanId();
}

/// <summary>
/// Activity scope for tracing
/// </summary>
public class ActivityScope : IDisposable
{
    private readonly Action _onDispose;

    public ActivityScope(Action onDispose)
    {
        _onDispose = onDispose;
    }

    public void Dispose()
    {
        _onDispose();
    }
}
