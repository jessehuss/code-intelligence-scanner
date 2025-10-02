using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Cataloger.Scanner.Analyzers;
using Cataloger.Scanner.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cataloger.Scanner.Tests.Unit;

public class POCOExtractorTests
{
    private readonly Mock<ILogger<POCOExtractor>> _mockLogger;
    private readonly POCOExtractor _extractor;

    public POCOExtractorTests()
    {
        _mockLogger = new Mock<ILogger<POCOExtractor>>();
        _extractor = new POCOExtractor(_mockLogger.Object);
    }

    [Fact]
    public void ExtractPOCOs_WithSimpleClass_ShouldReturnCodeType()
    {
        // Arrange
        var sourceCode = @"
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MyApp.Models
{
    public class User
    {
        [BsonId]
        public string Id { get; set; } = string.Empty;
        
        [BsonElement(""name"")]
        public string Name { get; set; } = string.Empty;
        
        public string Email { get; set; } = string.Empty;
    }
}";

        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var compilation = CSharpCompilation.Create("TestAssembly")
            .AddSyntaxTrees(syntaxTree)
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));

        // Act
        var result = _extractor.ExtractPOCOs(syntaxTree, compilation);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        var codeType = result.First();
        Assert.Equal("User", codeType.Name);
        Assert.Equal("MyApp.Models", codeType.Namespace);
        Assert.Equal(3, codeType.Fields.Count);
        
        var idField = codeType.Fields.First(f => f.Name == "Id");
        Assert.True(idField.BSONAttributes.Any(a => a.Name == "BsonId"));
        
        var nameField = codeType.Fields.First(f => f.Name == "Name");
        Assert.True(nameField.BSONAttributes.Any(a => a.Name == "BsonElement" && a.Value == "name"));
    }

    [Fact]
    public void ExtractPOCOs_WithComplexClass_ShouldReturnCodeTypeWithAllFields()
    {
        // Arrange
        var sourceCode = @"
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace MyApp.Models
{
    [BsonDiscriminator(""product"")]
    public class Product
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;
        
        [BsonElement(""product_name"")]
        public string Name { get; set; } = string.Empty;
        
        [BsonElement(""price"")]
        public decimal Price { get; set; }
        
        [BsonElement(""created_at"")]
        public DateTime CreatedAt { get; set; }
        
        [BsonIgnore]
        public string InternalNote { get; set; } = string.Empty;
        
        public List<string> Tags { get; set; } = new();
    }
}";

        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var compilation = CSharpCompilation.Create("TestAssembly")
            .AddSyntaxTrees(syntaxTree)
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));

        // Act
        var result = _extractor.ExtractPOCOs(syntaxTree, compilation);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        var codeType = result.First();
        Assert.Equal("Product", codeType.Name);
        Assert.Equal("MyApp.Models", codeType.Namespace);
        Assert.Equal(6, codeType.Fields.Count);
        
        // Check BSON attributes on class
        Assert.True(codeType.BSONAttributes.Any(a => a.Name == "BsonDiscriminator" && a.Value == "product"));
        
        // Check field attributes
        var idField = codeType.Fields.First(f => f.Name == "Id");
        Assert.True(idField.BSONAttributes.Any(a => a.Name == "BsonId"));
        Assert.True(idField.BSONAttributes.Any(a => a.Name == "BsonRepresentation"));
        
        var nameField = codeType.Fields.First(f => f.Name == "Name");
        Assert.True(nameField.BSONAttributes.Any(a => a.Name == "BsonElement" && a.Value == "product_name"));
        
        var internalNoteField = codeType.Fields.First(f => f.Name == "InternalNote");
        Assert.True(internalNoteField.BSONAttributes.Any(a => a.Name == "BsonIgnore"));
    }

    [Fact]
    public void ExtractPOCOs_WithNestedClass_ShouldReturnBothCodeTypes()
    {
        // Arrange
        var sourceCode = @"
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MyApp.Models
{
    public class Order
    {
        [BsonId]
        public string Id { get; set; } = string.Empty;
        
        public string CustomerId { get; set; } = string.Empty;
        
        public List<OrderItem> Items { get; set; } = new();
    }
    
    public class OrderItem
    {
        public string ProductId { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}";

        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var compilation = CSharpCompilation.Create("TestAssembly")
            .AddSyntaxTrees(syntaxTree)
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));

        // Act
        var result = _extractor.ExtractPOCOs(syntaxTree, compilation);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        
        var orderType = result.First(r => r.Name == "Order");
        Assert.Equal("Order", orderType.Name);
        Assert.Equal("MyApp.Models", orderType.Namespace);
        Assert.Equal(3, orderType.Fields.Count);
        
        var orderItemType = result.First(r => r.Name == "OrderItem");
        Assert.Equal("OrderItem", orderItemType.Name);
        Assert.Equal("MyApp.Models", orderItemType.Namespace);
        Assert.Equal(3, orderItemType.Fields.Count);
    }

    [Fact]
    public void ExtractPOCOs_WithGenericClass_ShouldReturnCodeType()
    {
        // Arrange
        var sourceCode = @"
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace MyApp.Models
{
    public class ApiResponse<T>
    {
        [BsonId]
        public string Id { get; set; } = string.Empty;
        
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T Data { get; set; } = default!;
        public DateTime Timestamp { get; set; }
    }
}";

        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var compilation = CSharpCompilation.Create("TestAssembly")
            .AddSyntaxTrees(syntaxTree)
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));

        // Act
        var result = _extractor.ExtractPOCOs(syntaxTree, compilation);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        var codeType = result.First();
        Assert.Equal("ApiResponse", codeType.Name);
        Assert.Equal("MyApp.Models", codeType.Namespace);
        Assert.Equal(5, codeType.Fields.Count);
    }

    [Fact]
    public void ExtractPOCOs_WithAbstractClass_ShouldNotReturnCodeType()
    {
        // Arrange
        var sourceCode = @"
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MyApp.Models
{
    public abstract class BaseEntity
    {
        [BsonId]
        public string Id { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}";

        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var compilation = CSharpCompilation.Create("TestAssembly")
            .AddSyntaxTrees(syntaxTree)
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));

        // Act
        var result = _extractor.ExtractPOCOs(syntaxTree, compilation);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void ExtractPOCOs_WithInterface_ShouldNotReturnCodeType()
    {
        // Arrange
        var sourceCode = @"
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MyApp.Models
{
    public interface IEntity
    {
        string Id { get; set; }
        DateTime CreatedAt { get; set; }
    }
}";

        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var compilation = CSharpCompilation.Create("TestAssembly")
            .AddSyntaxTrees(syntaxTree)
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));

        // Act
        var result = _extractor.ExtractPOCOs(syntaxTree, compilation);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void ExtractPOCOs_WithStaticClass_ShouldNotReturnCodeType()
    {
        // Arrange
        var sourceCode = @"
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MyApp.Models
{
    public static class Constants
    {
        public const string DefaultCollection = ""default"";
        public const int MaxRetries = 3;
    }
}";

        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var compilation = CSharpCompilation.Create("TestAssembly")
            .AddSyntaxTrees(syntaxTree)
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));

        // Act
        var result = _extractor.ExtractPOCOs(syntaxTree, compilation);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void ExtractPOCOs_WithNullableFields_ShouldSetNullabilityCorrectly()
    {
        // Arrange
        var sourceCode = @"
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace MyApp.Models
{
    public class User
    {
        [BsonId]
        public string Id { get; set; } = string.Empty;
        
        public string? Name { get; set; }
        public string Email { get; set; } = string.Empty;
        public DateTime? LastLogin { get; set; }
    }
}";

        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var compilation = CSharpCompilation.Create("TestAssembly")
            .AddSyntaxTrees(syntaxTree)
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));

        // Act
        var result = _extractor.ExtractPOCOs(syntaxTree, compilation);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        var codeType = result.First();
        Assert.Equal("User", codeType.Name);
        Assert.Equal(4, codeType.Fields.Count);
        
        var nameField = codeType.Fields.First(f => f.Name == "Name");
        Assert.True(nameField.IsNullable);
        
        var emailField = codeType.Fields.First(f => f.Name == "Email");
        Assert.False(emailField.IsNullable);
        
        var lastLoginField = codeType.Fields.First(f => f.Name == "LastLogin");
        Assert.True(lastLoginField.IsNullable);
    }

    [Fact]
    public void ExtractPOCOs_WithEmptyClass_ShouldReturnCodeTypeWithNoFields()
    {
        // Arrange
        var sourceCode = @"
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MyApp.Models
{
    public class EmptyClass
    {
    }
}";

        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var compilation = CSharpCompilation.Create("TestAssembly")
            .AddSyntaxTrees(syntaxTree)
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));

        // Act
        var result = _extractor.ExtractPOCOs(syntaxTree, compilation);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        var codeType = result.First();
        Assert.Equal("EmptyClass", codeType.Name);
        Assert.Equal("MyApp.Models", codeType.Namespace);
        Assert.Empty(codeType.Fields);
    }

    [Fact]
    public void ExtractPOCOs_WithMultipleNamespaces_ShouldReturnCodeTypesFromAllNamespaces()
    {
        // Arrange
        var sourceCode = @"
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MyApp.Models
{
    public class User
    {
        [BsonId]
        public string Id { get; set; } = string.Empty;
    }
}

namespace MyApp.Data
{
    public class Product
    {
        [BsonId]
        public string Id { get; set; } = string.Empty;
    }
}";

        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var compilation = CSharpCompilation.Create("TestAssembly")
            .AddSyntaxTrees(syntaxTree)
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));

        // Act
        var result = _extractor.ExtractPOCOs(syntaxTree, compilation);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        
        var userType = result.First(r => r.Name == "User");
        Assert.Equal("User", userType.Name);
        Assert.Equal("MyApp.Models", userType.Namespace);
        
        var productType = result.First(r => r.Name == "Product");
        Assert.Equal("Product", productType.Name);
        Assert.Equal("MyApp.Data", productType.Namespace);
    }
}
