using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace Cataloger.Scanner.Services;

/// <summary>
/// Performance monitoring service for the Code Intelligence Scanner.
/// </summary>
public class PerformanceMonitor
{
    private readonly ILogger<PerformanceMonitor> _logger;
    private readonly bool _enableDetailedMetrics;
    private readonly Dictionary<string, PerformanceMetrics> _metrics = new();
    private readonly object _lock = new();

    public PerformanceMonitor(ILogger<PerformanceMonitor> logger, bool enableDetailedMetrics = false)
    {
        _logger = logger;
        _enableDetailedMetrics = enableDetailedMetrics;
    }

    /// <summary>
    /// Starts monitoring a performance operation.
    /// </summary>
    /// <param name="operationName">Name of the operation to monitor.</param>
    /// <param name="context">Additional context for the operation.</param>
    /// <returns>Performance context for tracking the operation.</returns>
    public PerformanceContext StartOperation(string operationName, Dictionary<string, object>? context = null)
    {
        var performanceContext = new PerformanceContext
        {
            OperationName = operationName,
            StartTime = DateTime.UtcNow,
            StartMemory = GC.GetTotalMemory(false),
            Context = context ?? new Dictionary<string, object>()
        };

        if (_enableDetailedMetrics)
        {
            performanceContext.Stopwatch = Stopwatch.StartNew();
        }

        return performanceContext;
    }

    /// <summary>
    /// Completes monitoring of a performance operation.
    /// </summary>
    /// <param name="context">Performance context from StartOperation.</param>
    /// <returns>Performance metrics for the completed operation.</returns>
    public PerformanceMetrics CompleteOperation(PerformanceContext context)
    {
        var endTime = DateTime.UtcNow;
        var endMemory = GC.GetTotalMemory(false);
        var duration = endTime - context.StartTime;
        var memoryDelta = endMemory - context.StartMemory;

        var metrics = new PerformanceMetrics
        {
            OperationName = context.OperationName,
            StartTime = context.StartTime,
            EndTime = endTime,
            Duration = duration,
            StartMemory = context.StartMemory,
            EndMemory = endMemory,
            MemoryDelta = memoryDelta,
            Context = context.Context
        };

        if (context.Stopwatch != null)
        {
            context.Stopwatch.Stop();
            metrics.CpuTime = context.Stopwatch.Elapsed;
        }

        // Store metrics
        lock (_lock)
        {
            if (!_metrics.ContainsKey(context.OperationName))
            {
                _metrics[context.OperationName] = new PerformanceMetrics
                {
                    OperationName = context.OperationName
                };
            }

            var aggregateMetrics = _metrics[context.OperationName];
            aggregateMetrics.TotalDuration += duration;
            aggregateMetrics.TotalMemoryDelta += memoryDelta;
            aggregateMetrics.OperationCount++;
            aggregateMetrics.AverageDuration = aggregateMetrics.TotalDuration / aggregateMetrics.OperationCount;
            aggregateMetrics.AverageMemoryDelta = aggregateMetrics.TotalMemoryDelta / aggregateMetrics.OperationCount;

            if (duration > aggregateMetrics.MaxDuration)
            {
                aggregateMetrics.MaxDuration = duration;
            }

            if (duration < aggregateMetrics.MinDuration || aggregateMetrics.MinDuration == TimeSpan.Zero)
            {
                aggregateMetrics.MinDuration = duration;
            }

            if (memoryDelta > aggregateMetrics.MaxMemoryDelta)
            {
                aggregateMetrics.MaxMemoryDelta = memoryDelta;
            }

            if (memoryDelta < aggregateMetrics.MinMemoryDelta || aggregateMetrics.MinMemoryDelta == 0)
            {
                aggregateMetrics.MinMemoryDelta = memoryDelta;
            }
        }

        // Log performance metrics
        LogPerformanceMetrics(metrics);

        return metrics;
    }

    /// <summary>
    /// Records a performance event.
    /// </summary>
    /// <param name="eventName">Name of the event.</param>
    /// <param name="value">Value associated with the event.</param>
    /// <param name="context">Additional context.</param>
    public void RecordEvent(string eventName, double value, Dictionary<string, object>? context = null)
    {
        var eventData = new PerformanceEvent
        {
            EventName = eventName,
            Value = value,
            Timestamp = DateTime.UtcNow,
            Context = context ?? new Dictionary<string, object>()
        };

        _logger.LogInformation("Performance event: {EventName} = {Value} | {Context}",
            eventName, value, JsonSerializer.Serialize(context ?? new Dictionary<string, object>()));
    }

    /// <summary>
    /// Records a performance counter.
    /// </summary>
    /// <param name="counterName">Name of the counter.</param>
    /// <param name="increment">Increment value (default 1).</param>
    public void RecordCounter(string counterName, int increment = 1)
    {
        lock (_lock)
        {
            if (!_metrics.ContainsKey(counterName))
            {
                _metrics[counterName] = new PerformanceMetrics
                {
                    OperationName = counterName
                };
            }

            _metrics[counterName].OperationCount += increment;
        }

        _logger.LogDebug("Performance counter: {CounterName} += {Increment}",
            counterName, increment);
    }

    /// <summary>
    /// Gets current system performance metrics.
    /// </summary>
    /// <returns>System performance metrics.</returns>
    public SystemPerformanceMetrics GetSystemMetrics()
    {
        var process = Process.GetCurrentProcess();
        var workingSet = process.WorkingSet64;
        var privateMemory = process.PrivateMemorySize64;
        var virtualMemory = process.VirtualMemorySize64;
        var cpuTime = process.TotalProcessorTime;

        var systemMetrics = new SystemPerformanceMetrics
        {
            Timestamp = DateTime.UtcNow,
            WorkingSetBytes = workingSet,
            PrivateMemoryBytes = privateMemory,
            VirtualMemoryBytes = virtualMemory,
            CpuTime = cpuTime,
            ThreadCount = process.Threads.Count,
            HandleCount = process.HandleCount,
            GcMemory = GC.GetTotalMemory(false),
            GcGen0Collections = GC.CollectionCount(0),
            GcGen1Collections = GC.CollectionCount(1),
            GcGen2Collections = GC.CollectionCount(2)
        };

        return systemMetrics;
    }

    /// <summary>
    /// Gets aggregated performance metrics for an operation.
    /// </summary>
    /// <param name="operationName">Name of the operation.</param>
    /// <returns>Aggregated performance metrics.</returns>
    public PerformanceMetrics? GetAggregatedMetrics(string operationName)
    {
        lock (_lock)
        {
            return _metrics.TryGetValue(operationName, out var metrics) ? metrics : null;
        }
    }

    /// <summary>
    /// Gets all aggregated performance metrics.
    /// </summary>
    /// <returns>Dictionary of all performance metrics.</returns>
    public Dictionary<string, PerformanceMetrics> GetAllMetrics()
    {
        lock (_lock)
        {
            return new Dictionary<string, PerformanceMetrics>(_metrics);
        }
    }

    /// <summary>
    /// Clears all performance metrics.
    /// </summary>
    public void ClearMetrics()
    {
        lock (_lock)
        {
            _metrics.Clear();
        }

        _logger.LogInformation("Performance metrics cleared");
    }

    /// <summary>
    /// Checks if performance thresholds are exceeded.
    /// </summary>
    /// <param name="operationName">Name of the operation to check.</param>
    /// <param name="maxDuration">Maximum allowed duration.</param>
    /// <param name="maxMemoryDelta">Maximum allowed memory delta.</param>
    /// <returns>True if thresholds are exceeded.</returns>
    public bool CheckThresholds(string operationName, TimeSpan maxDuration, long maxMemoryDelta)
    {
        var metrics = GetAggregatedMetrics(operationName);
        if (metrics == null)
            return false;

        var exceedsDuration = metrics.AverageDuration > maxDuration;
        var exceedsMemory = Math.Abs(metrics.AverageMemoryDelta) > maxMemoryDelta;

        if (exceedsDuration || exceedsMemory)
        {
            _logger.LogWarning("Performance thresholds exceeded for {OperationName}: " +
                "Duration: {Duration} (max: {MaxDuration}), Memory: {Memory} (max: {MaxMemory})",
                operationName, metrics.AverageDuration, maxDuration, metrics.AverageMemoryDelta, maxMemoryDelta);
        }

        return exceedsDuration || exceedsMemory;
    }

    /// <summary>
    /// Generates a performance report.
    /// </summary>
    /// <returns>Performance report.</returns>
    public PerformanceReport GenerateReport()
    {
        var allMetrics = GetAllMetrics();
        var systemMetrics = GetSystemMetrics();

        var report = new PerformanceReport
        {
            GeneratedAt = DateTime.UtcNow,
            SystemMetrics = systemMetrics,
            OperationMetrics = allMetrics,
            Summary = new PerformanceSummary
            {
                TotalOperations = allMetrics.Values.Sum(m => m.OperationCount),
                AverageOperationDuration = allMetrics.Values.Any() 
                    ? TimeSpan.FromTicks((long)allMetrics.Values.Average(m => m.AverageDuration.Ticks))
                    : TimeSpan.Zero,
                TotalMemoryDelta = allMetrics.Values.Sum(m => m.TotalMemoryDelta),
                SlowestOperation = allMetrics.Values.OrderByDescending(m => m.MaxDuration).FirstOrDefault()?.OperationName,
                MostMemoryIntensiveOperation = allMetrics.Values.OrderByDescending(m => Math.Abs(m.MaxMemoryDelta)).FirstOrDefault()?.OperationName
            }
        };

        _logger.LogInformation("Performance report generated: {Summary}",
            JsonSerializer.Serialize(report.Summary));

        return report;
    }

    private void LogPerformanceMetrics(PerformanceMetrics metrics)
    {
        if (_enableDetailedMetrics)
        {
            _logger.LogInformation("Performance: {OperationName} | {Duration} | {MemoryDelta} | {Context}",
                metrics.OperationName, metrics.Duration, metrics.MemoryDelta,
                JsonSerializer.Serialize(metrics.Context));
        }
        else
        {
            _logger.LogDebug("Performance: {OperationName} completed in {Duration}ms",
                metrics.OperationName, metrics.Duration.TotalMilliseconds);
        }
    }
}

/// <summary>
/// Performance context for tracking an operation.
/// </summary>
public class PerformanceContext
{
    public string OperationName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public long StartMemory { get; set; }
    public Dictionary<string, object> Context { get; set; } = new();
    public Stopwatch? Stopwatch { get; set; }
}

/// <summary>
/// Performance metrics for an operation.
/// </summary>
public class PerformanceMetrics
{
    public string OperationName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan Duration { get; set; }
    public TimeSpan CpuTime { get; set; }
    public long StartMemory { get; set; }
    public long EndMemory { get; set; }
    public long MemoryDelta { get; set; }
    public Dictionary<string, object> Context { get; set; } = new();

    // Aggregated metrics
    public int OperationCount { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public TimeSpan AverageDuration { get; set; }
    public TimeSpan MaxDuration { get; set; }
    public TimeSpan MinDuration { get; set; }
    public long TotalMemoryDelta { get; set; }
    public long AverageMemoryDelta { get; set; }
    public long MaxMemoryDelta { get; set; }
    public long MinMemoryDelta { get; set; }
}

/// <summary>
/// System performance metrics.
/// </summary>
public class SystemPerformanceMetrics
{
    public DateTime Timestamp { get; set; }
    public long WorkingSetBytes { get; set; }
    public long PrivateMemoryBytes { get; set; }
    public long VirtualMemoryBytes { get; set; }
    public TimeSpan CpuTime { get; set; }
    public int ThreadCount { get; set; }
    public int HandleCount { get; set; }
    public long GcMemory { get; set; }
    public int GcGen0Collections { get; set; }
    public int GcGen1Collections { get; set; }
    public int GcGen2Collections { get; set; }
}

/// <summary>
/// Performance event.
/// </summary>
public class PerformanceEvent
{
    public string EventName { get; set; } = string.Empty;
    public double Value { get; set; }
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object> Context { get; set; } = new();
}

/// <summary>
/// Performance report.
/// </summary>
public class PerformanceReport
{
    public DateTime GeneratedAt { get; set; }
    public SystemPerformanceMetrics SystemMetrics { get; set; } = new();
    public Dictionary<string, PerformanceMetrics> OperationMetrics { get; set; } = new();
    public PerformanceSummary Summary { get; set; } = new();
}

/// <summary>
/// Performance summary.
/// </summary>
public class PerformanceSummary
{
    public int TotalOperations { get; set; }
    public TimeSpan AverageOperationDuration { get; set; }
    public long TotalMemoryDelta { get; set; }
    public string? SlowestOperation { get; set; }
    public string? MostMemoryIntensiveOperation { get; set; }
}
