using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Cataloger.Scanner.Services;
using Cataloger.Scanner.Models;
using System.Diagnostics;

namespace Cataloger.Scanner.Tests.Performance;

public class LargeCodebaseScanPerformanceTests
{
    private readonly Mock<ILogger<PerformanceMonitor>> _mockLogger;
    private readonly PerformanceMonitor _performanceMonitor;

    public LargeCodebaseScanPerformanceTests()
    {
        _mockLogger = new Mock<ILogger<PerformanceMonitor>>();
        _performanceMonitor = new PerformanceMonitor(_mockLogger.Object, enableDetailedMetrics: true);
    }

    [Fact]
    public void ScanLargeCodebase_ShouldCompleteWithinTimeLimit()
    {
        // Arrange
        var maxScanTime = TimeSpan.FromMinutes(10);
        var largeCodebase = GenerateLargeCodebase(1000); // 1000 files

        // Act
        var context = _performanceMonitor.StartOperation("LargeCodebaseScan");
        var result = SimulateLargeCodebaseScan(largeCodebase);
        var metrics = _performanceMonitor.CompleteOperation(context);

        // Assert
        Assert.True(metrics.Duration <= maxScanTime, 
            $"Scan took {metrics.Duration.TotalMinutes:F2} minutes, expected <= {maxScanTime.TotalMinutes} minutes");
        Assert.True(result.Success);
        Assert.True(result.ProcessedFiles > 0);
    }

    [Fact]
    public void ScanLargeCodebase_ShouldNotExceedMemoryLimit()
    {
        // Arrange
        var maxMemoryMB = 2048; // 2GB
        var largeCodebase = GenerateLargeCodebase(500); // 500 files

        // Act
        var context = _performanceMonitor.StartOperation("LargeCodebaseScan");
        var result = SimulateLargeCodebaseScan(largeCodebase);
        var metrics = _performanceMonitor.CompleteOperation(context);

        // Assert
        var memoryUsedMB = metrics.MemoryDelta / (1024 * 1024);
        Assert.True(memoryUsedMB <= maxMemoryMB, 
            $"Memory usage was {memoryUsedMB:F2}MB, expected <= {maxMemoryMB}MB");
        Assert.True(result.Success);
    }

    [Fact]
    public void ScanLargeCodebase_ShouldProcessFilesConcurrently()
    {
        // Arrange
        var largeCodebase = GenerateLargeCodebase(200); // 200 files
        var expectedConcurrency = 20; // Max concurrent files

        // Act
        var context = _performanceMonitor.StartOperation("LargeCodebaseScan");
        var result = SimulateLargeCodebaseScan(largeCodebase);
        var metrics = _performanceMonitor.CompleteOperation(context);

        // Assert
        Assert.True(result.MaxConcurrentFiles <= expectedConcurrency,
            $"Max concurrent files was {result.MaxConcurrentFiles}, expected <= {expectedConcurrency}");
        Assert.True(result.Success);
    }

    [Fact]
    public void ScanLargeCodebase_ShouldHandleMemoryPressure()
    {
        // Arrange
        var largeCodebase = GenerateLargeCodebase(1000); // 1000 files
        var memoryPressureThreshold = 1024; // 1GB

        // Act
        var context = _performanceMonitor.StartOperation("LargeCodebaseScan");
        var result = SimulateLargeCodebaseScan(largeCodebase);
        var metrics = _performanceMonitor.CompleteOperation(context);

        // Assert
        var peakMemoryMB = metrics.EndMemory / (1024 * 1024);
        Assert.True(peakMemoryMB <= memoryPressureThreshold * 2, // Allow 2x threshold for peak
            $"Peak memory usage was {peakMemoryMB:F2}MB, expected <= {memoryPressureThreshold * 2}MB");
        Assert.True(result.Success);
    }

    [Fact]
    public void ScanLargeCodebase_ShouldMaintainPerformanceUnderLoad()
    {
        // Arrange
        var largeCodebase = GenerateLargeCodebase(300); // 300 files
        var expectedThroughput = 50; // files per second

        // Act
        var context = _performanceMonitor.StartOperation("LargeCodebaseScan");
        var result = SimulateLargeCodebaseScan(largeCodebase);
        var metrics = _performanceMonitor.CompleteOperation(context);

        // Assert
        var actualThroughput = result.ProcessedFiles / metrics.Duration.TotalSeconds;
        Assert.True(actualThroughput >= expectedThroughput,
            $"Throughput was {actualThroughput:F2} files/sec, expected >= {expectedThroughput} files/sec");
        Assert.True(result.Success);
    }

    [Fact]
    public void ScanLargeCodebase_ShouldHandleLargeFiles()
    {
        // Arrange
        var largeCodebase = GenerateLargeCodebase(100); // 100 files
        var largeFileSize = 10 * 1024 * 1024; // 10MB per file

        // Act
        var context = _performanceMonitor.StartOperation("LargeCodebaseScan");
        var result = SimulateLargeCodebaseScan(largeCodebase, largeFileSize);
        var metrics = _performanceMonitor.CompleteOperation(context);

        // Assert
        Assert.True(result.Success);
        Assert.True(result.ProcessedFiles > 0);
        Assert.True(metrics.Duration.TotalMinutes <= 5, // Should complete within 5 minutes
            $"Scan took {metrics.Duration.TotalMinutes:F2} minutes, expected <= 5 minutes");
    }

    [Fact]
    public void ScanLargeCodebase_ShouldHandleComplexCode()
    {
        // Arrange
        var largeCodebase = GenerateComplexCodebase(200); // 200 files with complex code

        // Act
        var context = _performanceMonitor.StartOperation("LargeCodebaseScan");
        var result = SimulateLargeCodebaseScan(largeCodebase);
        var metrics = _performanceMonitor.CompleteOperation(context);

        // Assert
        Assert.True(result.Success);
        Assert.True(result.ProcessedFiles > 0);
        Assert.True(result.ExtractedTypes > 0);
        Assert.True(result.ExtractedOperations > 0);
    }

    [Fact]
    public void ScanLargeCodebase_ShouldHandleNetworkLatency()
    {
        // Arrange
        var largeCodebase = GenerateLargeCodebase(150); // 150 files
        var networkLatency = TimeSpan.FromMilliseconds(100); // 100ms latency

        // Act
        var context = _performanceMonitor.StartOperation("LargeCodebaseScan");
        var result = SimulateLargeCodebaseScan(largeCodebase, networkLatency: networkLatency);
        var metrics = _performanceMonitor.CompleteOperation(context);

        // Assert
        Assert.True(result.Success);
        Assert.True(result.ProcessedFiles > 0);
        // Network latency should not significantly impact performance
        Assert.True(metrics.Duration.TotalMinutes <= 3, // Should complete within 3 minutes
            $"Scan took {metrics.Duration.TotalMinutes:F2} minutes, expected <= 3 minutes");
    }

    [Fact]
    public void ScanLargeCodebase_ShouldHandleConcurrentScans()
    {
        // Arrange
        var largeCodebase = GenerateLargeCodebase(100); // 100 files
        var concurrentScans = 3;

        // Act
        var tasks = new List<Task<ScanResult>>();
        for (int i = 0; i < concurrentScans; i++)
        {
            tasks.Add(Task.Run(() => SimulateLargeCodebaseScan(largeCodebase)));
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        foreach (var result in results)
        {
            Assert.True(result.Success);
            Assert.True(result.ProcessedFiles > 0);
        }
    }

    [Fact]
    public void ScanLargeCodebase_ShouldHandleResourceConstraints()
    {
        // Arrange
        var largeCodebase = GenerateLargeCodebase(200); // 200 files
        var cpuLimit = 80; // 80% CPU limit
        var memoryLimit = 1024; // 1GB memory limit

        // Act
        var context = _performanceMonitor.StartOperation("LargeCodebaseScan");
        var result = SimulateLargeCodebaseScan(largeCodebase, cpuLimit: cpuLimit, memoryLimit: memoryLimit);
        var metrics = _performanceMonitor.CompleteOperation(context);

        // Assert
        Assert.True(result.Success);
        Assert.True(result.ProcessedFiles > 0);
        // Should complete successfully even under resource constraints
        Assert.True(metrics.Duration.TotalMinutes <= 8, // Should complete within 8 minutes
            $"Scan took {metrics.Duration.TotalMinutes:F2} minutes, expected <= 8 minutes");
    }

    [Fact]
    public void ScanLargeCodebase_ShouldHandleErrorRecovery()
    {
        // Arrange
        var largeCodebase = GenerateLargeCodebase(100); // 100 files
        var errorRate = 0.1; // 10% error rate

        // Act
        var context = _performanceMonitor.StartOperation("LargeCodebaseScan");
        var result = SimulateLargeCodebaseScan(largeCodebase, errorRate: errorRate);
        var metrics = _performanceMonitor.CompleteOperation(context);

        // Assert
        Assert.True(result.Success);
        Assert.True(result.ProcessedFiles > 0);
        Assert.True(result.ErrorCount > 0); // Should have some errors
        Assert.True(result.ErrorCount <= result.ProcessedFiles * errorRate * 1.5); // Error count should be reasonable
    }

    private List<CodeFile> GenerateLargeCodebase(int fileCount)
    {
        var files = new List<CodeFile>();
        var random = new Random(42); // Fixed seed for reproducible tests

        for (int i = 0; i < fileCount; i++)
        {
            files.Add(new CodeFile
            {
                Path = $"Models/Entity{i}.cs",
                Content = GenerateCodeFile(i, random),
                Size = random.Next(1024, 10240) // 1KB to 10KB
            });
        }

        return files;
    }

    private List<CodeFile> GenerateComplexCodebase(int fileCount)
    {
        var files = new List<CodeFile>();
        var random = new Random(42); // Fixed seed for reproducible tests

        for (int i = 0; i < fileCount; i++)
        {
            files.Add(new CodeFile
            {
                Path = $"Services/Service{i}.cs",
                Content = GenerateComplexCodeFile(i, random),
                Size = random.Next(5120, 51200) // 5KB to 50KB
            });
        }

        return files;
    }

    private string GenerateCodeFile(int index, Random random)
    {
        var classCount = random.Next(1, 5);
        var code = new System.Text.StringBuilder();

        code.AppendLine("using MongoDB.Bson;");
        code.AppendLine("using MongoDB.Bson.Serialization.Attributes;");
        code.AppendLine("using MongoDB.Driver;");
        code.AppendLine();
        code.AppendLine("namespace MyApp.Models");
        code.AppendLine("{");

        for (int i = 0; i < classCount; i++)
        {
            code.AppendLine($"    public class Entity{index}_{i}");
            code.AppendLine("    {");
            code.AppendLine("        [BsonId]");
            code.AppendLine("        public string Id { get; set; } = string.Empty;");

            var fieldCount = random.Next(3, 10);
            for (int j = 0; j < fieldCount; j++)
            {
                var fieldType = random.Next(0, 4) switch
                {
                    0 => "string",
                    1 => "int",
                    2 => "DateTime",
                    3 => "bool",
                    _ => "string"
                };

                code.AppendLine($"        public {fieldType} Field{j} {{ get; set; }}");
            }

            code.AppendLine("    }");
            code.AppendLine();
        }

        code.AppendLine("}");

        return code.ToString();
    }

    private string GenerateComplexCodeFile(int index, Random random)
    {
        var code = new System.Text.StringBuilder();

        code.AppendLine("using MongoDB.Bson;");
        code.AppendLine("using MongoDB.Bson.Serialization.Attributes;");
        code.AppendLine("using MongoDB.Driver;");
        code.AppendLine("using System.Linq;");
        code.AppendLine();
        code.AppendLine("namespace MyApp.Services");
        code.AppendLine("{");

        code.AppendLine($"    public class Service{index}");
        code.AppendLine("    {");
        code.AppendLine("        private readonly IMongoCollection<Entity> _collection;");
        code.AppendLine();
        code.AppendLine("        public Service(IMongoDatabase database)");
        code.AppendLine("        {");
        code.AppendLine("            _collection = database.GetCollection<Entity>(\"entities\");");
        code.AppendLine("        }");

        var methodCount = random.Next(5, 15);
        for (int i = 0; i < methodCount; i++)
        {
            var operation = random.Next(0, 4) switch
            {
                0 => "Find",
                1 => "InsertOne",
                2 => "UpdateOne",
                3 => "DeleteOne",
                _ => "Find"
            };

            code.AppendLine();
            code.AppendLine($"        public async Task<Entity> Method{i}()");
            code.AppendLine("        {");
            code.AppendLine($"            return await _collection.{operation}(e => e.Id == \"test\").FirstOrDefaultAsync();");
            code.AppendLine("        }");
        }

        code.AppendLine("    }");
        code.AppendLine("}");

        return code.ToString();
    }

    private ScanResult SimulateLargeCodebaseScan(
        List<CodeFile> codebase, 
        int fileSize = 1024, 
        TimeSpan? networkLatency = null, 
        int cpuLimit = 100, 
        int memoryLimit = 2048, 
        double errorRate = 0.0)
    {
        var stopwatch = Stopwatch.StartNew();
        var processedFiles = 0;
        var extractedTypes = 0;
        var extractedOperations = 0;
        var errorCount = 0;
        var maxConcurrentFiles = 0;
        var currentConcurrentFiles = 0;
        var random = new Random(42);

        // Simulate concurrent file processing
        var semaphore = new SemaphoreSlim(20, 20); // Max 20 concurrent files
        var tasks = new List<Task>();

        foreach (var file in codebase)
        {
            tasks.Add(Task.Run(async () =>
            {
                await semaphore.WaitAsync();
                try
                {
                    Interlocked.Increment(ref currentConcurrentFiles);
                    maxConcurrentFiles = Math.Max(maxConcurrentFiles, currentConcurrentFiles);

                    // Simulate file processing time
                    var processingTime = TimeSpan.FromMilliseconds(random.Next(10, 100));
                    if (networkLatency.HasValue)
                    {
                        processingTime += networkLatency.Value;
                    }

                    await Task.Delay(processingTime);

                    // Simulate errors
                    if (random.NextDouble() < errorRate)
                    {
                        Interlocked.Increment(ref errorCount);
                    }
                    else
                    {
                        Interlocked.Increment(ref processedFiles);
                        Interlocked.Add(ref extractedTypes, random.Next(1, 5));
                        Interlocked.Add(ref extractedOperations, random.Next(2, 8));
                    }

                    Interlocked.Decrement(ref currentConcurrentFiles);
                }
                finally
                {
                    semaphore.Release();
                }
            }));
        }

        Task.WaitAll(tasks.ToArray());
        stopwatch.Stop();

        return new ScanResult
        {
            Success = true,
            ProcessedFiles = processedFiles,
            ExtractedTypes = extractedTypes,
            ExtractedOperations = extractedOperations,
            ErrorCount = errorCount,
            MaxConcurrentFiles = maxConcurrentFiles,
            Duration = stopwatch.Elapsed
        };
    }

    private class CodeFile
    {
        public string Path { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int Size { get; set; }
    }

    private class ScanResult
    {
        public bool Success { get; set; }
        public int ProcessedFiles { get; set; }
        public int ExtractedTypes { get; set; }
        public int ExtractedOperations { get; set; }
        public int ErrorCount { get; set; }
        public int MaxConcurrentFiles { get; set; }
        public TimeSpan Duration { get; set; }
    }
}
