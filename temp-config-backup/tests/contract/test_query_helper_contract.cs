using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace CatalogExplorer.Tests.Contract
{
    public class QueryHelperContractTests
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public QueryHelperContractTests()
        {
            _httpClient = new HttpClient();
            _baseUrl = Environment.GetEnvironmentVariable("API_BASE_URL") ?? "http://localhost:8080/api";
        }

        [Fact]
        public async Task GenerateQuery_WithValidParameters_ReturnsQueryExamples()
        {
            // Arrange
            var typeName = "User";
            var fieldPath = "email";
            var operation = "FIND";
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/query-helper?type={typeName}&field={fieldPath}&operation={operation}");

            // Act
            var response = await _httpClient.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode, $"Query helper API returned {response.StatusCode}");
            
            var content = await response.Content.ReadAsStringAsync();
            Assert.NotEmpty(content);
            
            // Verify response structure
            Assert.Contains("mongoShell", content);
            Assert.Contains("csharpBuilder", content);
            Assert.Contains("operation", content);
            Assert.Contains("fieldPath", content);
        }

        [Fact]
        public async Task GenerateQuery_WithNestedFieldPath_ReturnsQueryExamples()
        {
            // Arrange
            var typeName = "User";
            var fieldPath = "profile.name";
            var operation = "FIND";
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/query-helper?type={typeName}&field={fieldPath}&operation={operation}");

            // Act
            var response = await _httpClient.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode, $"Query helper API with nested field returned {response.StatusCode}");
            
            var content = await response.Content.ReadAsStringAsync();
            Assert.NotEmpty(content);
            Assert.Contains("mongoShell", content);
            Assert.Contains("csharpBuilder", content);
        }

        [Fact]
        public async Task GenerateQuery_WithAllOperations_ReturnsQueryExamples()
        {
            // Arrange
            var typeName = "User";
            var fieldPath = "email";
            var operations = new[] { "FIND", "INSERT", "UPDATE", "DELETE", "AGGREGATE" };

            foreach (var operation in operations)
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/query-helper?type={typeName}&field={fieldPath}&operation={operation}");

                // Act
                var response = await _httpClient.SendAsync(request);

                // Assert
                Assert.True(response.IsSuccessStatusCode, $"Query helper API for {operation} returned {response.StatusCode}");
                
                var content = await response.Content.ReadAsStringAsync();
                Assert.NotEmpty(content);
                Assert.Contains("mongoShell", content);
                Assert.Contains("csharpBuilder", content);
            }
        }

        [Fact]
        public async Task GenerateQuery_WithInvalidOperation_ReturnsBadRequest()
        {
            // Arrange
            var typeName = "User";
            var fieldPath = "email";
            var operation = "INVALID_OPERATION";
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/query-helper?type={typeName}&field={fieldPath}&operation={operation}");

            // Act
            var response = await _httpClient.SendAsync(request);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GenerateQuery_WithMissingType_ReturnsBadRequest()
        {
            // Arrange
            var fieldPath = "email";
            var operation = "FIND";
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/query-helper?field={fieldPath}&operation={operation}");

            // Act
            var response = await _httpClient.SendAsync(request);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GenerateQuery_WithMissingField_ReturnsBadRequest()
        {
            // Arrange
            var typeName = "User";
            var operation = "FIND";
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/query-helper?type={typeName}&operation={operation}");

            // Act
            var response = await _httpClient.SendAsync(request);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GenerateQuery_WithMissingOperation_ReturnsBadRequest()
        {
            // Arrange
            var typeName = "User";
            var fieldPath = "email";
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/query-helper?type={typeName}&field={fieldPath}");

            // Act
            var response = await _httpClient.SendAsync(request);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GenerateQuery_ResponseTime_IsUnder200ms()
        {
            // Arrange
            var typeName = "User";
            var fieldPath = "email";
            var operation = "FIND";
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/query-helper?type={typeName}&field={fieldPath}&operation={operation}");
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            var response = await _httpClient.SendAsync(request);
            stopwatch.Stop();

            // Assert
            Assert.True(response.IsSuccessStatusCode, $"Query helper API returned {response.StatusCode}");
            Assert.True(stopwatch.ElapsedMilliseconds < 200, $"Query helper request took {stopwatch.ElapsedMilliseconds}ms, expected < 200ms");
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
