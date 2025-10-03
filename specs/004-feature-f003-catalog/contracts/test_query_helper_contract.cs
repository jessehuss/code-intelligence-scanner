using System.Net;
using System.Text.Json;
using Xunit;

namespace CatalogExplorer.Contracts.Tests;

public class QueryHelperContractTests
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public QueryHelperContractTests()
    {
        _client = new HttpClient();
        _client.BaseAddress = new Uri("http://localhost:3000/api");
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    [Fact]
    public async Task GenerateQueryExamples_WithValidRequest_ReturnsExamples()
    {
        // Arrange
        var request = new QueryHelperRequest
        {
            TypePath = "User",
            FieldPath = "Email",
            Operation = "FIND"
        };

        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/query-helper", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var queryResponse = JsonSerializer.Deserialize<QueryHelperResponse>(responseContent, _jsonOptions);
        
        Assert.NotNull(queryResponse);
        Assert.False(string.IsNullOrEmpty(queryResponse.MongoShell));
        Assert.False(string.IsNullOrEmpty(queryResponse.CsharpBuilder));
    }

    [Fact]
    public async Task GenerateQueryExamples_WithInsertOperation_ReturnsInsertExamples()
    {
        // Arrange
        var request = new QueryHelperRequest
        {
            TypePath = "Product",
            FieldPath = "Name",
            Operation = "INSERT"
        };

        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/query-helper", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var queryResponse = JsonSerializer.Deserialize<QueryHelperResponse>(responseContent, _jsonOptions);
        
        Assert.NotNull(queryResponse);
        Assert.Contains("insert", queryResponse.MongoShell.ToLower());
        Assert.Contains("Insert", queryResponse.CsharpBuilder);
    }

    [Fact]
    public async Task GenerateQueryExamples_WithUpdateOperation_ReturnsUpdateExamples()
    {
        // Arrange
        var request = new QueryHelperRequest
        {
            TypePath = "Order",
            FieldPath = "Status",
            Operation = "UPDATE"
        };

        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/query-helper", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var queryResponse = JsonSerializer.Deserialize<QueryHelperResponse>(responseContent, _jsonOptions);
        
        Assert.NotNull(queryResponse);
        Assert.Contains("update", queryResponse.MongoShell.ToLower());
        Assert.Contains("Update", queryResponse.CsharpBuilder);
    }

    [Fact]
    public async Task GenerateQueryExamples_WithDeleteOperation_ReturnsDeleteExamples()
    {
        // Arrange
        var request = new QueryHelperRequest
        {
            TypePath = "TempData",
            FieldPath = "Id",
            Operation = "DELETE"
        };

        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/query-helper", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var queryResponse = JsonSerializer.Deserialize<QueryHelperResponse>(responseContent, _jsonOptions);
        
        Assert.NotNull(queryResponse);
        Assert.Contains("delete", queryResponse.MongoShell.ToLower());
        Assert.Contains("Delete", queryResponse.CsharpBuilder);
    }

    [Fact]
    public async Task GenerateQueryExamples_WithAggregateOperation_ReturnsAggregateExamples()
    {
        // Arrange
        var request = new QueryHelperRequest
        {
            TypePath = "Sales",
            FieldPath = "Amount",
            Operation = "AGGREGATE"
        };

        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/query-helper", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var queryResponse = JsonSerializer.Deserialize<QueryHelperResponse>(responseContent, _jsonOptions);
        
        Assert.NotNull(queryResponse);
        Assert.Contains("aggregate", queryResponse.MongoShell.ToLower());
        Assert.Contains("Aggregate", queryResponse.CsharpBuilder);
    }

    [Fact]
    public async Task GenerateQueryExamples_WithEmptyTypePath_ReturnsBadRequest()
    {
        // Arrange
        var request = new QueryHelperRequest
        {
            TypePath = "",
            FieldPath = "Email",
            Operation = "FIND"
        };

        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/query-helper", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GenerateQueryExamples_WithEmptyFieldPath_ReturnsBadRequest()
    {
        // Arrange
        var request = new QueryHelperRequest
        {
            TypePath = "User",
            FieldPath = "",
            Operation = "FIND"
        };

        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/query-helper", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GenerateQueryExamples_WithInvalidOperation_ReturnsBadRequest()
    {
        // Arrange
        var request = new QueryHelperRequest
        {
            TypePath = "User",
            FieldPath = "Email",
            Operation = "INVALID"
        };

        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/query-helper", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GenerateQueryExamples_WithNestedFieldPath_ReturnsValidExamples()
    {
        // Arrange
        var request = new QueryHelperRequest
        {
            TypePath = "User",
            FieldPath = "Profile.Address.City",
            Operation = "FIND"
        };

        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/query-helper", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var queryResponse = JsonSerializer.Deserialize<QueryHelperResponse>(responseContent, _jsonOptions);
        
        Assert.NotNull(queryResponse);
        Assert.Contains("Profile.Address.City", queryResponse.MongoShell);
        Assert.Contains("Profile.Address.City", queryResponse.CsharpBuilder);
    }

    [Fact]
    public async Task GenerateQueryExamples_ReturnsExecutableCode()
    {
        // Arrange
        var request = new QueryHelperRequest
        {
            TypePath = "User",
            FieldPath = "Email",
            Operation = "FIND"
        };

        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/query-helper", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var queryResponse = JsonSerializer.Deserialize<QueryHelperResponse>(responseContent, _jsonOptions);
        
        Assert.NotNull(queryResponse);
        
        // Validate Mongo shell example
        Assert.Contains("db.", queryResponse.MongoShell);
        Assert.Contains("find", queryResponse.MongoShell.ToLower());
        
        // Validate C# Builder example
        Assert.Contains("Builders<", queryResponse.CsharpBuilder);
        Assert.Contains("Filter", queryResponse.CsharpBuilder);
    }
}

// Contract models
public class QueryHelperRequest
{
    public string TypePath { get; set; } = string.Empty;
    public string FieldPath { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
}

public class QueryHelperResponse
{
    public string MongoShell { get; set; } = string.Empty;
    public string CsharpBuilder { get; set; } = string.Empty;
}
