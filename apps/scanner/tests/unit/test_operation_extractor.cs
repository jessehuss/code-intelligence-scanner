using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Cataloger.Scanner.Analyzers;
using Cataloger.Scanner.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cataloger.Scanner.Tests.Unit;

public class OperationExtractorTests
{
    private readonly Mock<ILogger<OperationExtractor>> _mockLogger;
    private readonly OperationExtractor _extractor;

    public OperationExtractorTests()
    {
        _mockLogger = new Mock<ILogger<OperationExtractor>>();
        _extractor = new OperationExtractor(_mockLogger.Object);
    }

    [Fact]
    public void ExtractOperations_WithFindOperation_ShouldReturnQueryOperation()
    {
        // Arrange
        var sourceCode = @"
using MongoDB.Driver;
using System.Linq;

namespace MyApp.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _users;
        
        public UserService(IMongoDatabase database)
        {
            _users = database.GetCollection<User>(""users"");
        }
        
        public async Task<User?> GetUserById(string id)
        {
            return await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
        }
    }
}";

        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var compilation = CSharpCompilation.Create("TestAssembly")
            .AddSyntaxTrees(syntaxTree)
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));

        // Act
        var result = _extractor.ExtractOperations(syntaxTree, compilation);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        var operation = result.First();
        Assert.Equal("Find", operation.OperationType);
        Assert.Equal("users", operation.CollectionId);
        Assert.NotNull(operation.Filters);
    }

    [Fact]
    public void ExtractOperations_WithUpdateOperation_ShouldReturnQueryOperation()
    {
        // Arrange
        var sourceCode = @"
using MongoDB.Driver;
using System.Linq;

namespace MyApp.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _users;
        
        public UserService(IMongoDatabase database)
        {
            _users = database.GetCollection<User>(""users"");
        }
        
        public async Task<bool> UpdateUser(string id, string name)
        {
            var result = await _users.UpdateOneAsync(
                u => u.Id == id,
                Builders<User>.Update.Set(u => u.Name, name)
            );
            return result.ModifiedCount > 0;
        }
    }
}";

        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var compilation = CSharpCompilation.Create("TestAssembly")
            .AddSyntaxTrees(syntaxTree)
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));

        // Act
        var result = _extractor.ExtractOperations(syntaxTree, compilation);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        var operation = result.First();
        Assert.Equal("UpdateOne", operation.OperationType);
        Assert.Equal("users", operation.CollectionId);
        Assert.NotNull(operation.Filters);
    }

    [Fact]
    public void ExtractOperations_WithInsertOperation_ShouldReturnQueryOperation()
    {
        // Arrange
        var sourceCode = @"
using MongoDB.Driver;
using System.Linq;

namespace MyApp.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _users;
        
        public UserService(IMongoDatabase database)
        {
            _users = database.GetCollection<User>(""users"");
        }
        
        public async Task<User> CreateUser(User user)
        {
            await _users.InsertOneAsync(user);
            return user;
        }
    }
}";

        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var compilation = CSharpCompilation.Create("TestAssembly")
            .AddSyntaxTrees(syntaxTree)
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));

        // Act
        var result = _extractor.ExtractOperations(syntaxTree, compilation);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        var operation = result.First();
        Assert.Equal("InsertOne", operation.OperationType);
        Assert.Equal("users", operation.CollectionId);
    }

    [Fact]
    public void ExtractOperations_WithDeleteOperation_ShouldReturnQueryOperation()
    {
        // Arrange
        var sourceCode = @"
using MongoDB.Driver;
using System.Linq;

namespace MyApp.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _users;
        
        public UserService(IMongoDatabase database)
        {
            _users = database.GetCollection<User>(""users"");
        }
        
        public async Task<bool> DeleteUser(string id)
        {
            var result = await _users.DeleteOneAsync(u => u.Id == id);
            return result.DeletedCount > 0;
        }
    }
}";

        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var compilation = CSharpCompilation.Create("TestAssembly")
            .AddSyntaxTrees(syntaxTree)
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));

        // Act
        var result = _extractor.ExtractOperations(syntaxTree, compilation);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        var operation = result.First();
        Assert.Equal("DeleteOne", operation.OperationType);
        Assert.Equal("users", operation.CollectionId);
        Assert.NotNull(operation.Filters);
    }

    [Fact]
    public void ExtractOperations_WithAggregateOperation_ShouldReturnQueryOperation()
    {
        // Arrange
        var sourceCode = @"
using MongoDB.Driver;
using System.Linq;

namespace MyApp.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _users;
        
        public UserService(IMongoDatabase database)
        {
            _users = database.GetCollection<User>(""users"");
        }
        
        public async Task<List<User>> GetActiveUsers()
        {
            return await _users.Aggregate()
                .Match(u => u.IsActive)
                .Sort(u => u.Name)
                .Limit(100)
                .ToListAsync();
        }
    }
}";

        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var compilation = CSharpCompilation.Create("TestAssembly")
            .AddSyntaxTrees(syntaxTree)
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));

        // Act
        var result = _extractor.ExtractOperations(syntaxTree, compilation);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        var operation = result.First();
        Assert.Equal("Aggregate", operation.OperationType);
        Assert.Equal("users", operation.CollectionId);
        Assert.NotEmpty(operation.AggregationPipeline);
    }

    [Fact]
    public void ExtractOperations_WithReplaceOperation_ShouldReturnQueryOperation()
    {
        // Arrange
        var sourceCode = @"
using MongoDB.Driver;
using System.Linq;

namespace MyApp.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _users;
        
        public UserService(IMongoDatabase database)
        {
            _users = database.GetCollection<User>(""users"");
        }
        
        public async Task<bool> ReplaceUser(User user)
        {
            var result = await _users.ReplaceOneAsync(u => u.Id == user.Id, user);
            return result.ModifiedCount > 0;
        }
    }
}";

        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var compilation = CSharpCompilation.Create("TestAssembly")
            .AddSyntaxTrees(syntaxTree)
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));

        // Act
        var result = _extractor.ExtractOperations(syntaxTree, compilation);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        var operation = result.First();
        Assert.Equal("ReplaceOne", operation.OperationType);
        Assert.Equal("users", operation.CollectionId);
        Assert.NotNull(operation.Filters);
    }

    [Fact]
    public void ExtractOperations_WithMultipleOperations_ShouldReturnAllOperations()
    {
        // Arrange
        var sourceCode = @"
using MongoDB.Driver;
using System.Linq;

namespace MyApp.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _users;
        
        public UserService(IMongoDatabase database)
        {
            _users = database.GetCollection<User>(""users"");
        }
        
        public async Task<User?> GetUserById(string id)
        {
            return await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
        }
        
        public async Task<User> CreateUser(User user)
        {
            await _users.InsertOneAsync(user);
            return user;
        }
        
        public async Task<bool> UpdateUser(string id, string name)
        {
            var result = await _users.UpdateOneAsync(
                u => u.Id == id,
                Builders<User>.Update.Set(u => u.Name, name)
            );
            return result.ModifiedCount > 0;
        }
    }
}";

        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var compilation = CSharpCompilation.Create("TestAssembly")
            .AddSyntaxTrees(syntaxTree)
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));

        // Act
        var result = _extractor.ExtractOperations(syntaxTree, compilation);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
        
        var findOperation = result.First(r => r.OperationType == "Find");
        Assert.Equal("Find", findOperation.OperationType);
        Assert.Equal("users", findOperation.CollectionId);
        
        var insertOperation = result.First(r => r.OperationType == "InsertOne");
        Assert.Equal("InsertOne", insertOperation.OperationType);
        Assert.Equal("users", insertOperation.CollectionId);
        
        var updateOperation = result.First(r => r.OperationType == "UpdateOne");
        Assert.Equal("UpdateOne", updateOperation.OperationType);
        Assert.Equal("users", updateOperation.CollectionId);
    }

    [Fact]
    public void ExtractOperations_WithProjection_ShouldReturnQueryOperationWithProjection()
    {
        // Arrange
        var sourceCode = @"
using MongoDB.Driver;
using System.Linq;

namespace MyApp.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _users;
        
        public UserService(IMongoDatabase database)
        {
            _users = database.GetCollection<User>(""users"");
        }
        
        public async Task<List<User>> GetUserNames()
        {
            return await _users.Find(u => u.IsActive)
                .Project(u => new User { Id = u.Id, Name = u.Name })
                .ToListAsync();
        }
    }
}";

        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var compilation = CSharpCompilation.Create("TestAssembly")
            .AddSyntaxTrees(syntaxTree)
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));

        // Act
        var result = _extractor.ExtractOperations(syntaxTree, compilation);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        var operation = result.First();
        Assert.Equal("Find", operation.OperationType);
        Assert.Equal("users", operation.CollectionId);
        Assert.NotNull(operation.Projections);
    }

    [Fact]
    public void ExtractOperations_WithSortAndLimit_ShouldReturnQueryOperationWithSortAndLimit()
    {
        // Arrange
        var sourceCode = @"
using MongoDB.Driver;
using System.Linq;

namespace MyApp.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _users;
        
        public UserService(IMongoDatabase database)
        {
            _users = database.GetCollection<User>(""users"");
        }
        
        public async Task<List<User>> GetTopUsers(int count)
        {
            return await _users.Find(u => u.IsActive)
                .Sort(u => u.Score)
                .Limit(count)
                .ToListAsync();
        }
    }
}";

        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var compilation = CSharpCompilation.Create("TestAssembly")
            .AddSyntaxTrees(syntaxTree)
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));

        // Act
        var result = _extractor.ExtractOperations(syntaxTree, compilation);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        var operation = result.First();
        Assert.Equal("Find", operation.OperationType);
        Assert.Equal("users", operation.CollectionId);
        Assert.NotNull(operation.Sort);
        Assert.Equal(10, operation.Limit); // Assuming default limit
    }

    [Fact]
    public void ExtractOperations_WithSkip_ShouldReturnQueryOperationWithSkip()
    {
        // Arrange
        var sourceCode = @"
using MongoDB.Driver;
using System.Linq;

namespace MyApp.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _users;
        
        public UserService(IMongoDatabase database)
        {
            _users = database.GetCollection<User>(""users"");
        }
        
        public async Task<List<User>> GetUsersPage(int page, int pageSize)
        {
            return await _users.Find(u => u.IsActive)
                .Skip(page * pageSize)
                .Limit(pageSize)
                .ToListAsync();
        }
    }
}";

        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var compilation = CSharpCompilation.Create("TestAssembly")
            .AddSyntaxTrees(syntaxTree)
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));

        // Act
        var result = _extractor.ExtractOperations(syntaxTree, compilation);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        var operation = result.First();
        Assert.Equal("Find", operation.OperationType);
        Assert.Equal("users", operation.CollectionId);
        Assert.NotNull(operation.Skip);
        Assert.NotNull(operation.Limit);
    }

    [Fact]
    public void ExtractOperations_WithNoOperations_ShouldReturnEmptyList()
    {
        // Arrange
        var sourceCode = @"
namespace MyApp.Services
{
    public class UserService
    {
        public string GetServiceName()
        {
            return ""UserService"";
        }
    }
}";

        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var compilation = CSharpCompilation.Create("TestAssembly")
            .AddSyntaxTrees(syntaxTree)
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));

        // Act
        var result = _extractor.ExtractOperations(syntaxTree, compilation);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void ExtractOperations_WithComplexFilter_ShouldReturnQueryOperationWithComplexFilter()
    {
        // Arrange
        var sourceCode = @"
using MongoDB.Driver;
using System.Linq;

namespace MyApp.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _users;
        
        public UserService(IMongoDatabase database)
        {
            _users = database.GetCollection<User>(""users"");
        }
        
        public async Task<List<User>> GetUsersByCriteria(string name, int minAge, bool isActive)
        {
            return await _users.Find(u => 
                u.Name.Contains(name) && 
                u.Age >= minAge && 
                u.IsActive == isActive)
                .ToListAsync();
        }
    }
}";

        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var compilation = CSharpCompilation.Create("TestAssembly")
            .AddSyntaxTrees(syntaxTree)
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));

        // Act
        var result = _extractor.ExtractOperations(syntaxTree, compilation);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        var operation = result.First();
        Assert.Equal("Find", operation.OperationType);
        Assert.Equal("users", operation.CollectionId);
        Assert.NotNull(operation.Filters);
    }
}
