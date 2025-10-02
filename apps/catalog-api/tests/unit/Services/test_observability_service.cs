using CatalogApi.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CatalogApi.Tests.Unit.Services;

/// <summary>
/// Unit tests for ObservabilityService
/// </summary>
public class TestObservabilityService
{
    private readonly Mock<ILogger<ObservabilityService>> _mockLogger;
    private readonly ObservabilityService _service;

    public TestObservabilityService()
    {
        _mockLogger = new Mock<ILogger<ObservabilityService>>();
        _service = new ObservabilityService(_mockLogger.Object);
    }

    [Fact]
    public void LogInformation_WithMessage_CallsLoggerLogInformation()
    {
        // Arrange
        var message = "Test information message";
        var arg1 = "arg1";
        var arg2 = 42;

        _mockLogger.Setup(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

        // Act
        _service.LogInformation(message, arg1, arg2);

        // Assert
        _mockLogger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public void LogInformation_WithNoArgs_CallsLoggerLogInformation()
    {
        // Arrange
        var message = "Test information message without args";

        _mockLogger.Setup(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

        // Act
        _service.LogInformation(message);

        // Assert
        _mockLogger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public void LogWarning_WithMessage_CallsLoggerLogWarning()
    {
        // Arrange
        var message = "Test warning message";
        var arg1 = "warning_arg";
        var arg2 = 100;

        _mockLogger.Setup(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

        // Act
        _service.LogWarning(message, arg1, arg2);

        // Assert
        _mockLogger.Verify(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public void LogWarning_WithNoArgs_CallsLoggerLogWarning()
    {
        // Arrange
        var message = "Test warning message without args";

        _mockLogger.Setup(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

        // Act
        _service.LogWarning(message);

        // Assert
        _mockLogger.Verify(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public void LogError_WithException_CallsLoggerLogError()
    {
        // Arrange
        var exception = new InvalidOperationException("Test exception");
        var message = "Test error message";
        var arg1 = "error_arg";
        var arg2 = 500;

        _mockLogger.Setup(x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

        // Act
        _service.LogError(exception, message, arg1, arg2);

        // Assert
        _mockLogger.Verify(x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            exception,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public void LogError_WithExceptionAndNoArgs_CallsLoggerLogError()
    {
        // Arrange
        var exception = new ArgumentException("Test argument exception");
        var message = "Test error message without args";

        _mockLogger.Setup(x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

        // Act
        _service.LogError(exception, message);

        // Assert
        _mockLogger.Verify(x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            exception,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public void LogError_WithNullException_CallsLoggerLogError()
    {
        // Arrange
        Exception? exception = null;
        var message = "Test error message with null exception";
        var arg1 = "null_exception_arg";

        _mockLogger.Setup(x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

        // Act
        _service.LogError(exception, message, arg1);

        // Assert
        _mockLogger.Verify(x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            exception,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public void BeginScope_WithNameAndArgs_ReturnsDisposableScope()
    {
        // Arrange
        var scopeName = "TestScope";
        var arg1 = "scope_arg1";
        var arg2 = 123;

        var mockScope = new Mock<IDisposable>();
        _mockLogger.Setup(x => x.BeginScope(It.IsAny<object>()))
            .Returns(mockScope.Object);

        // Act
        var scope = _service.BeginScope(scopeName, arg1, arg2);

        // Assert
        Assert.NotNull(scope);
        Assert.Equal(mockScope.Object, scope);
        _mockLogger.Verify(x => x.BeginScope(It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public void BeginScope_WithNameOnly_ReturnsDisposableScope()
    {
        // Arrange
        var scopeName = "SimpleScope";

        var mockScope = new Mock<IDisposable>();
        _mockLogger.Setup(x => x.BeginScope(It.IsAny<object>()))
            .Returns(mockScope.Object);

        // Act
        var scope = _service.BeginScope(scopeName);

        // Assert
        Assert.NotNull(scope);
        Assert.Equal(mockScope.Object, scope);
        _mockLogger.Verify(x => x.BeginScope(It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public void Measure_WithMetricNameAndValue_CallsLoggerLogInformation()
    {
        // Arrange
        var metricName = "test_metric";
        var value = 42.5;

        _mockLogger.Setup(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

        // Act
        _service.Measure(metricName, value);

        // Assert
        _mockLogger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public void Measure_WithMetricNameValueAndDimensions_CallsLoggerLogInformation()
    {
        // Arrange
        var metricName = "test_metric_with_dimensions";
        var value = 100.0;
        var dimensions = new Dictionary<string, string>
        {
            { "dimension1", "value1" },
            { "dimension2", "value2" }
        };

        _mockLogger.Setup(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

        // Act
        _service.Measure(metricName, value, dimensions);

        // Assert
        _mockLogger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public void Measure_WithNullDimensions_CallsLoggerLogInformation()
    {
        // Arrange
        var metricName = "test_metric_null_dimensions";
        var value = 75.25;
        Dictionary<string, string>? dimensions = null;

        _mockLogger.Setup(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

        // Act
        _service.Measure(metricName, value, dimensions);

        // Assert
        _mockLogger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public void Measure_WithEmptyDimensions_CallsLoggerLogInformation()
    {
        // Arrange
        var metricName = "test_metric_empty_dimensions";
        var value = 0.0;
        var dimensions = new Dictionary<string, string>();

        _mockLogger.Setup(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

        // Act
        _service.Measure(metricName, value, dimensions);

        // Assert
        _mockLogger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public void Measure_WithNegativeValue_CallsLoggerLogInformation()
    {
        // Arrange
        var metricName = "test_metric_negative";
        var value = -10.5;

        _mockLogger.Setup(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

        // Act
        _service.Measure(metricName, value);

        // Assert
        _mockLogger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public void Measure_WithZeroValue_CallsLoggerLogInformation()
    {
        // Arrange
        var metricName = "test_metric_zero";
        var value = 0.0;

        _mockLogger.Setup(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

        // Act
        _service.Measure(metricName, value);

        // Assert
        _mockLogger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public void Measure_WithLargeValue_CallsLoggerLogInformation()
    {
        // Arrange
        var metricName = "test_metric_large";
        var value = 999999.999;

        _mockLogger.Setup(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

        // Act
        _service.Measure(metricName, value);

        // Assert
        _mockLogger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public void Measure_WithSpecialCharactersInMetricName_CallsLoggerLogInformation()
    {
        // Arrange
        var metricName = "test_metric_with_special_chars_!@#$%^&*()";
        var value = 123.456;

        _mockLogger.Setup(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

        // Act
        _service.Measure(metricName, value);

        // Assert
        _mockLogger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }
}
