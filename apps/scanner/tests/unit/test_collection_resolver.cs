using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Cataloger.Scanner.Resolvers;
using Cataloger.Scanner.Models;

namespace Cataloger.Scanner.Tests.Unit;

public class CollectionResolverTests
{
    private readonly Mock<ILogger<CollectionResolver>> _mockLogger;
    private readonly CollectionResolver _resolver;

    public CollectionResolverTests()
    {
        _mockLogger = new Mock<ILogger<CollectionResolver>>();
        _resolver = new CollectionResolver(_mockLogger.Object);
    }

    [Fact]
    public void ResolveCollectionName_WithLiteralString_ShouldReturnLiteralValue()
    {
        // Arrange
        var codeType = new CodeType
        {
            Name = "User",
            Namespace = "MyApp.Models"
        };

        var collectionName = "users";

        // Act
        var result = _resolver.ResolveCollectionName(codeType, collectionName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(collectionName, result.CollectionName);
        Assert.Equal("literal", result.ResolutionMethod);
        Assert.Equal(1.0, result.Confidence);
    }

    [Fact]
    public void ResolveCollectionName_WithConstant_ShouldReturnConstantValue()
    {
        // Arrange
        var codeType = new CodeType
        {
            Name = "Product",
            Namespace = "MyApp.Models"
        };

        var constantName = "PRODUCT_COLLECTION";

        // Act
        var result = _resolver.ResolveCollectionName(codeType, constantName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(constantName, result.CollectionName);
        Assert.Equal("constant", result.ResolutionMethod);
        Assert.Equal(0.8, result.Confidence);
    }

    [Fact]
    public void ResolveCollectionName_WithConfiguration_ShouldReturnConfigValue()
    {
        // Arrange
        var codeType = new CodeType
        {
            Name = "Order",
            Namespace = "MyApp.Models"
        };

        var configKey = "Collections:Orders";

        // Act
        var result = _resolver.ResolveCollectionName(codeType, configKey);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(configKey, result.CollectionName);
        Assert.Equal("config", result.ResolutionMethod);
        Assert.Equal(0.7, result.Confidence);
    }

    [Fact]
    public void ResolveCollectionName_WithInferredName_ShouldReturnInferredValue()
    {
        // Arrange
        var codeType = new CodeType
        {
            Name = "Customer",
            Namespace = "MyApp.Models"
        };

        // Act
        var result = _resolver.ResolveCollectionName(codeType, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("customers", result.CollectionName);
        Assert.Equal("inferred", result.ResolutionMethod);
        Assert.Equal(0.6, result.Confidence);
    }

    [Fact]
    public void ResolveCollectionName_WithNullType_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _resolver.ResolveCollectionName(null, "test"));
    }

    [Fact]
    public void ResolveCollectionName_WithEmptyTypeName_ShouldReturnDefaultCollection()
    {
        // Arrange
        var codeType = new CodeType
        {
            Name = "",
            Namespace = "MyApp.Models"
        };

        // Act
        var result = _resolver.ResolveCollectionName(codeType, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("documents", result.CollectionName);
        Assert.Equal("inferred", result.ResolutionMethod);
        Assert.Equal(0.3, result.Confidence);
    }

    [Fact]
    public void ResolveCollectionName_WithSpecialCharacters_ShouldSanitizeName()
    {
        // Arrange
        var codeType = new CodeType
        {
            Name = "UserProfile",
            Namespace = "MyApp.Models"
        };

        // Act
        var result = _resolver.ResolveCollectionName(codeType, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("userprofiles", result.CollectionName);
        Assert.Equal("inferred", result.ResolutionMethod);
        Assert.Equal(0.6, result.Confidence);
    }

    [Fact]
    public void ResolveCollectionName_WithPluralization_ShouldReturnPluralForm()
    {
        // Arrange
        var codeType = new CodeType
        {
            Name = "Category",
            Namespace = "MyApp.Models"
        };

        // Act
        var result = _resolver.ResolveCollectionName(codeType, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("categories", result.CollectionName);
        Assert.Equal("inferred", result.ResolutionMethod);
        Assert.Equal(0.6, result.Confidence);
    }

    [Fact]
    public void ResolveCollectionName_WithComplexTypeName_ShouldHandleCorrectly()
    {
        // Arrange
        var codeType = new CodeType
        {
            Name = "UserAccountSettings",
            Namespace = "MyApp.Models"
        };

        // Act
        var result = _resolver.ResolveCollectionName(codeType, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("useraccountsettings", result.CollectionName);
        Assert.Equal("inferred", result.ResolutionMethod);
        Assert.Equal(0.6, result.Confidence);
    }

    [Fact]
    public void ResolveCollectionName_WithHighConfidenceLiteral_ShouldReturnHighConfidence()
    {
        // Arrange
        var codeType = new CodeType
        {
            Name = "Product",
            Namespace = "MyApp.Models"
        };

        var collectionName = "products";

        // Act
        var result = _resolver.ResolveCollectionName(codeType, collectionName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(collectionName, result.CollectionName);
        Assert.Equal("literal", result.ResolutionMethod);
        Assert.Equal(1.0, result.Confidence);
    }

    [Fact]
    public void ResolveCollectionName_WithMediumConfidenceConstant_ShouldReturnMediumConfidence()
    {
        // Arrange
        var codeType = new CodeType
        {
            Name = "Order",
            Namespace = "MyApp.Models"
        };

        var constantName = "ORDERS_COLLECTION";

        // Act
        var result = _resolver.ResolveCollectionName(codeType, constantName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(constantName, result.CollectionName);
        Assert.Equal("constant", result.ResolutionMethod);
        Assert.Equal(0.8, result.Confidence);
    }

    [Fact]
    public void ResolveCollectionName_WithLowConfidenceConfig_ShouldReturnLowConfidence()
    {
        // Arrange
        var codeType = new CodeType
        {
            Name = "Customer",
            Namespace = "MyApp.Models"
        };

        var configKey = "Collections:Customers";

        // Act
        var result = _resolver.ResolveCollectionName(codeType, configKey);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(configKey, result.CollectionName);
        Assert.Equal("config", result.ResolutionMethod);
        Assert.Equal(0.7, result.Confidence);
    }

    [Fact]
    public void ResolveCollectionName_WithVeryLowConfidenceInferred_ShouldReturnVeryLowConfidence()
    {
        // Arrange
        var codeType = new CodeType
        {
            Name = "Unknown",
            Namespace = "MyApp.Models"
        };

        // Act
        var result = _resolver.ResolveCollectionName(codeType, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("unknowns", result.CollectionName);
        Assert.Equal("inferred", result.ResolutionMethod);
        Assert.Equal(0.6, result.Confidence);
    }
}
