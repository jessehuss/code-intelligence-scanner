using CatalogApi.Services;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using System.Text.Json;
using Xunit;

namespace CatalogApi.Tests.Unit.Services;

/// <summary>
/// Unit tests for CacheService
/// </summary>
public class TestCacheService
{
    private readonly Mock<IDistributedCache> _mockCache;
    private readonly Mock<ILogger<CacheService>> _mockLogger;
    private readonly CacheService _service;

    public TestCacheService()
    {
        _mockCache = new Mock<IDistributedCache>();
        _mockLogger = new Mock<ILogger<CacheService>>();
        _service = new CacheService(_mockCache.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetOrCreateAsync_WithCacheHit_ReturnsCachedValue()
    {
        // Arrange
        var key = "test-key";
        var cachedValue = "cached-data";
        var expectedResult = new TestModel { Id = 1, Name = "Test" };

        _mockCache.Setup(x => x.GetStringAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync(JsonSerializer.Serialize(expectedResult));

        // Act
        var result = await _service.GetOrCreateAsync(key, () => Task.FromResult(new TestModel { Id = 2, Name = "Factory" }));

        // Assert
        Assert.Equal(expectedResult.Id, result.Id);
        Assert.Equal(expectedResult.Name, result.Name);
        _mockCache.Verify(x => x.GetStringAsync(key, It.IsAny<CancellationToken>()), Times.Once);
        _mockCache.Verify(x => x.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetOrCreateAsync_WithCacheMiss_CallsFactoryAndCachesResult()
    {
        // Arrange
        var key = "test-key";
        var factoryResult = new TestModel { Id = 2, Name = "Factory" };

        _mockCache.Setup(x => x.GetStringAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        _mockCache.Setup(x => x.SetStringAsync(key, It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.GetOrCreateAsync(key, () => Task.FromResult(factoryResult));

        // Assert
        Assert.Equal(factoryResult.Id, result.Id);
        Assert.Equal(factoryResult.Name, result.Name);
        _mockCache.Verify(x => x.GetStringAsync(key, It.IsAny<CancellationToken>()), Times.Once);
        _mockCache.Verify(x => x.SetStringAsync(key, It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetOrCreateAsync_WithCustomTtl_UsesCustomTtl()
    {
        // Arrange
        var key = "test-key";
        var factoryResult = new TestModel { Id = 3, Name = "CustomTTL" };
        var customTtl = TimeSpan.FromMinutes(15);

        _mockCache.Setup(x => x.GetStringAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        _mockCache.Setup(x => x.SetStringAsync(key, It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.GetOrCreateAsync(key, () => Task.FromResult(factoryResult), customTtl);

        // Assert
        Assert.Equal(factoryResult.Id, result.Id);
        _mockCache.Verify(x => x.SetStringAsync(key, It.IsAny<string>(), 
            It.Is<DistributedCacheEntryOptions>(o => o.AbsoluteExpirationRelativeToNow == customTtl), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetOrCreateAsync_WithNullFactoryResult_DoesNotCache()
    {
        // Arrange
        var key = "test-key";

        _mockCache.Setup(x => x.GetStringAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        // Act
        var result = await _service.GetOrCreateAsync<TestModel?>(key, () => Task.FromResult((TestModel?)null));

        // Assert
        Assert.Null(result);
        _mockCache.Verify(x => x.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetOrCreateAsync_WithFactoryException_PropagatesException()
    {
        // Arrange
        var key = "test-key";
        var expectedException = new InvalidOperationException("Factory error");

        _mockCache.Setup(x => x.GetStringAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.GetOrCreateAsync(key, () => throw expectedException));

        Assert.Equal("Factory error", exception.Message);
        _mockCache.Verify(x => x.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetOrCreateAsync_WithInvalidJson_ThrowsJsonException()
    {
        // Arrange
        var key = "test-key";
        var invalidJson = "invalid-json";

        _mockCache.Setup(x => x.GetStringAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invalidJson);

        // Act & Assert
        await Assert.ThrowsAsync<JsonException>(() =>
            _service.GetOrCreateAsync(key, () => Task.FromResult(new TestModel { Id = 1, Name = "Test" })));
    }

    [Fact]
    public async Task RemoveAsync_CallsCacheRemove()
    {
        // Arrange
        var key = "test-key";

        _mockCache.Setup(x => x.RemoveAsync(key, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.RemoveAsync(key);

        // Assert
        _mockCache.Verify(x => x.RemoveAsync(key, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ClearAllAsync_LogsWarningAndCompletes()
    {
        // Arrange
        _mockLogger.Setup(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

        // Act
        await _service.ClearAllAsync();

        // Assert
        _mockLogger.Verify(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public async Task GetOrCreateAsync_WithComplexObject_SerializesAndDeserializesCorrectly()
    {
        // Arrange
        var key = "complex-key";
        var complexObject = new ComplexTestModel
        {
            Id = 1,
            Name = "Complex",
            Properties = new Dictionary<string, object>
            {
                { "Prop1", "Value1" },
                { "Prop2", 42 },
                { "Prop3", true }
            },
            NestedObject = new TestModel { Id = 2, Name = "Nested" },
            ListProperty = new List<string> { "Item1", "Item2", "Item3" }
        };

        _mockCache.Setup(x => x.GetStringAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        _mockCache.Setup(x => x.SetStringAsync(key, It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.GetOrCreateAsync(key, () => Task.FromResult(complexObject));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(complexObject.Id, result.Id);
        Assert.Equal(complexObject.Name, result.Name);
        Assert.Equal(complexObject.Properties.Count, result.Properties.Count);
        Assert.Equal(complexObject.Properties["Prop1"], result.Properties["Prop1"]);
        Assert.Equal(complexObject.Properties["Prop2"], result.Properties["Prop2"]);
        Assert.Equal(complexObject.Properties["Prop3"], result.Properties["Prop3"]);
        Assert.NotNull(result.NestedObject);
        Assert.Equal(complexObject.NestedObject.Id, result.NestedObject.Id);
        Assert.Equal(complexObject.NestedObject.Name, result.NestedObject.Name);
        Assert.Equal(complexObject.ListProperty.Count, result.ListProperty.Count);
        Assert.Equal(complexObject.ListProperty[0], result.ListProperty[0]);
    }

    [Fact]
    public async Task GetOrCreateAsync_WithDefaultTtl_UsesDefaultTtl()
    {
        // Arrange
        var key = "test-key";
        var factoryResult = new TestModel { Id = 4, Name = "DefaultTTL" };
        var defaultTtl = TimeSpan.FromMinutes(30);

        _mockCache.Setup(x => x.GetStringAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        _mockCache.Setup(x => x.SetStringAsync(key, It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.GetOrCreateAsync(key, () => Task.FromResult(factoryResult));

        // Assert
        Assert.Equal(factoryResult.Id, result.Id);
        _mockCache.Verify(x => x.SetStringAsync(key, It.IsAny<string>(), 
            It.Is<DistributedCacheEntryOptions>(o => o.AbsoluteExpirationRelativeToNow == defaultTtl), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetOrCreateAsync_WithEmptyKey_ThrowsArgumentException()
    {
        // Arrange
        var key = "";
        var factoryResult = new TestModel { Id = 5, Name = "EmptyKey" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.GetOrCreateAsync(key, () => Task.FromResult(factoryResult)));
    }

    [Fact]
    public async Task GetOrCreateAsync_WithNullKey_ThrowsArgumentNullException()
    {
        // Arrange
        string? key = null;
        var factoryResult = new TestModel { Id = 6, Name = "NullKey" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.GetOrCreateAsync(key, () => Task.FromResult(factoryResult)));
    }

    [Fact]
    public async Task GetOrCreateAsync_WithNullFactory_ThrowsArgumentNullException()
    {
        // Arrange
        var key = "test-key";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.GetOrCreateAsync(key, null!));
    }

    // Test helper classes
    public class TestModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class ComplexTestModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Dictionary<string, object> Properties { get; set; } = new();
        public TestModel? NestedObject { get; set; }
        public List<string> ListProperty { get; set; } = new();
    }
}
